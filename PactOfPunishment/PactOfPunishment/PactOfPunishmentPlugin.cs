using BepInEx;
using BepInEx.Configuration;
using HG;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using PactOfPunishment.Conditions;
using PactOfPunishment.Waves.Infrastructure;
using PactOfPunishment.Waves.Stage1.Halcyonites;
using PactOfPunishment.Waves.Stage3;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace PactOfPunishment
{
    [BepInPlugin("com.woodyscales.pactofpunishment", "Pact of Punishment", "0.0.1")]
    public partial class PactOfPunishmentPlugin : BaseUnityPlugin
    {
        private const string Section = "Conditions";

        private readonly List<Module> modules = new List<Module>();

        private readonly Dictionary<IConditionDef, ConfigEntry<int>> conditionDefs = new Dictionary<IConditionDef, ConfigEntry<int>>();

        public void Awake()
        {
            // It's ok to use reflection here as Awake will only be called once when the plugin is loaded.
            var moduleTypes = this.GetType().Assembly
                .GetTypes()
                .Where(t =>
                    typeof(Module).IsAssignableFrom(t) &&
                    t.IsClass &&
                    !t.IsAbstract);
            var tempModules = GetModules(moduleTypes).ToList();

            foreach (var module in tempModules)
            {
                try
                {
                    module.Logger = this.Logger;
                    module.Init();
                    this.Logger.LogInfo($"Initialized module: {module.GetType().Name}");
                }
                catch (Exception ex)
                {
                    this.Logger.LogWarning(ex);
                    continue;
                }

                this.modules.Add(module);
            }

            this.SetupConditions();

            On.RoR2.Run.Awake += this.Run_Awake;
            IL.RoR2.CombatDirector.AttemptSpawnOnTarget += Utils.HookIL(CombatDirector_AttemptSpawnOnTarget);
            IL.RoR2.InfiniteTowerWaveController.FixedUpdate += Utils.HookIL(this.InfiniteTowerWaveController_FixedUpdate);
            Summoner2.Summoner2BossFightBehavior.eggSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/Junk/Incubator/cscParentPod.asset"); // TODO: move to its own module?
            Summoner2.Summoner2BossFightBehavior.parentSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/Base/Parent/cscParent.asset"); // TODO: hook global spawn card event instead and get spawn card from spawn request
        }

        private static void CombatDirector_AttemptSpawnOnTarget(ILCursor c)
        {
            int num2VariableNumber = -1, numVariableNumber = -1;

            c.GotoNext(
                x => x.MatchLdloc(out _),
                x => x.MatchConvR4(),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<CombatDirector>(nameof(CombatDirector.monsterCredit)),
                x => x.MatchBgtUn(out _),

                x => x.MatchLdloc(out _),
                x => x.MatchStloc(out numVariableNumber),

                x => x.MatchLdarg(0),
                x => x.MatchLdfld<CombatDirector>(nameof(CombatDirector.currentActiveEliteTier)),
                x => x.MatchLdfld<CombatDirector.EliteTierDef>(nameof(CombatDirector.EliteTierDef.costMultiplier)),
                x => x.MatchStloc(out num2VariableNumber),

                x => x.MatchBr(out _)
            );

            c.GotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchCall<CombatDirector>(nameof(CombatDirector.ResetEliteType)),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<CombatDirector>(nameof(CombatDirector.currentActiveEliteDef)),
                x => x.MatchStloc(out _));
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<CombatDirector, float>>(self => self.currentActiveEliteTier.costMultiplier);
            c.Emit(OpCodes.Dup);
            c.Emit(OpCodes.Stloc_S, (byte)num2VariableNumber);
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<float, CombatDirector, int>>((eliteTierCostMultiplier, self) => (int)(self.currentMonsterCard.cost * eliteTierCostMultiplier));
            c.Emit(OpCodes.Stloc_S, (byte)numVariableNumber);

            c.Index = 0;
            while (c.TryGotoNext(MoveType.AfterLabel, x => x.MatchLdfld<CombatDirector>(nameof(CombatDirector.currentActiveEliteTier))))
            {
                c.Remove();
                c.EmitDelegate<Func<CombatDirector, CombatDirector.EliteTierDef>>(self =>
                {
                    if (self.currentActiveEliteTier is null)
                    {
                        self.ResetEliteType();
                    }

                    return self.currentActiveEliteTier!;
                });
            }
        }

        private static string SplitPascalCaseString(string input)
        {
            var output = new StringBuilder();

            foreach (char c in input)
            {
                if (char.IsUpper(c))
                {
                    output.Append(' ');
                }

                output.Append(c);
            }

            return output.ToString().Trim();
        }

        [ConCommand(commandName = "simulacrum_complete_wave", flags = ConVarFlags.ExecuteOnServer, helpText = "Completes the current simulacrum wave.")]
        private static void CmdCompleteSimulacrumWave(ConCommandArgs args)
        {
            if (Run.instance is InfiniteTowerRun run && run.waveController is InfiniteTowerWaveController wave)
            {
                wave.Network_totalWaveCredits = wave.combatDirector.totalCreditsSpent;

                var teamMembers = TeamComponent.GetTeamMembers(TeamIndex.Monster);
                for (int k = teamMembers.Count - 1; k >= 0; k--)
                {
                    teamMembers[k].body.master?.TrueKill(wave.gameObject, wave.gameObject, DamageType.VoidDeath);
                }
            }
        }

        [ConCommand(commandName = "simulacrum_override_wave", flags = ConVarFlags.ExecuteOnServer, helpText = "Overrides the next simulacrum wave.")]
        private static void CmdOverrideNextSimulacrumWave(ConCommandArgs args)
        {
            if (Run.instance is InfiniteTowerRun run && run.GetComponent<SimulacrumWavesBehavior>() is SimulacrumWavesBehavior behavior && args.Count > 0)
            {
                behavior.WaveOverrideName = args[0];
            }
        }

        private IEnumerable<Module> GetModules(IEnumerable<Type> moduleTypes)
        {
            foreach (var moduleType in moduleTypes)
            {
                // 1. Check for public static Instance property
                var instanceProperty = moduleType.GetProperty(
                    "Instance",
                    BindingFlags.Public | BindingFlags.Static);

                if (instanceProperty != null && moduleType.IsAssignableFrom(instanceProperty.PropertyType))
                {
                    var value = instanceProperty.GetValue(null);
                    if (value != null)
                    {
                        yield return (Module)value;
                        continue;
                    }
                }

                // 2. Otherwise try public parameterless constructor
                var ctor = moduleType.GetConstructor(Type.EmptyTypes);
                if (ctor != null)
                {
                    yield return (Module)Activator.CreateInstance(moduleType);
                    continue;
                }

                this.Logger.LogWarning($"Could not find a way to instantiate module of type {moduleType.FullName}. Ensure it has either a public static Instance property or a public parameterless constructor.");
            }
        }
        private void InfiniteTowerWaveController_FixedUpdate(ILCursor c)
        {
            while (c.TryGotoNext(MoveType.AfterLabel,
                x => x.MatchLdfld<InfiniteTowerWaveController>(nameof(InfiniteTowerWaveController.combatDirector)),
                x => x.MatchLdcR4(0),
                x => x.MatchStfld<CombatDirector>(nameof(CombatDirector.monsterCredit))))
            {
                c.RemoveRange(3);
                c.EmitDelegate<Action<InfiniteTowerWaveController>>(self =>
                {
                    if (self.GetComponent<KeepCombatDirectorEnabledBehavior>()?.enabled == true)
                    {
                        return;
                    }

                    self.combatDirector.monsterCredit = 0;
                });
            }
        }

        private void Run_Awake(On.RoR2.Run.orig_Awake orig, Run self)
        {
            self.EnsureComponent<PactOfPunishmentBehavior>().SetConditions(this.conditionDefs.Select(x => new Condition(x.Key, x.Value.Value)));
            orig(self);
        }

        private void SetupConditions()
        {
            foreach (var conditionDef in this.modules.OfType<IConditionDef>())
            {
                var configEntry = this.Config.Bind(Section, SplitPascalCaseString(conditionDef.GetType().Name), 0);
                ModSettingsManager.AddOption(new IntSliderOption(configEntry, new IntSliderConfig { restartRequired = false, min = 0, max = conditionDef.MaxRank, checkIfDisabled = () => Run.instance }));
                this.conditionDefs[conditionDef] = configEntry;
            }
        }
    }
}
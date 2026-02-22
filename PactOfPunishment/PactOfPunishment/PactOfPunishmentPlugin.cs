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
using System.Text;

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
            var tempModules = new Module[]
            {
                new HardLabor(),
                new LastingConsequences(),
                new ConvenienceFee(),
                new JurySummons(),
                new ExtremeMeasures(),
                new CalisthenicsProgram(),
                new BenefitsPackage(),
                new MiddleManagement(),
                new UnderworldCustoms(),
                new ForcedOvertime(),
                new HeightenedSecurity(),
                new RoutineInspection(),
                new DamageControl(),
                new ApprovalProcess(),
                new TightDeadline(),
                new PersonalLiability(),
                SimulacrumWavesModule.Instance,
                new IncreaseSpawnRateWhileThereAreNoMonsters(),
                new WaveUpgradesModule(),
                RecalculateStats.Instance,
                new HalcyoniteModule(),
                EliteTiers.Instance,
                new MoveSafeWardFaster(),
                new DisplayFullKillerNameInRunReport(),
                new DisableCombatDirectorWhileSquadFull(),
                new ImproveChildMonsterAI(),
            };

            foreach (var module in tempModules)
            {
                try
                {
                    module.Logger = this.Logger;
                    module.Init();
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
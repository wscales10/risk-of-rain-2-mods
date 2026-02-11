using BepInEx;
using BepInEx.Configuration;
using HG;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using PactOfPunishment.Conditions;
using PactOfPunishment.Waves;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                new SimulacrumWaves(),
                new KeepEliteDefOverride(),
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
        }

        private static void CombatDirector_AttemptSpawnOnTarget(ILCursor c)
        {
            c.GotoNext(MoveType.AfterLabel,
                x => x.MatchLdfld<CombatDirector>(nameof(CombatDirector.currentActiveEliteTier)));
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
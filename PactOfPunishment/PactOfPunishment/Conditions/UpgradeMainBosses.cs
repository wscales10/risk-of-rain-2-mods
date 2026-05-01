using HG;
using PactOfPunishment.Waves.FinalStage;
using PactOfPunishment.Waves.Infrastructure;
using RoR2;

namespace PactOfPunishment.Conditions
{
    [ModuleDependency(typeof(EncounterUpgradeModule))]
    public sealed class UpgradeMainBosses : ConditionDef
    {
        public override int MaxRank => 4;

        public static bool IsMainBossWave()
        {
            return Run.instance is InfiniteTowerRun run && run.waveController && run.IsStageTransitionWave() && run.waveController.isBossWave; // TODO: this may be incorrect for stage 4+?
        }

        public override int GetHeatForRank(int rank) => rank;

        public override void Init()
        {
            EncounterUpgradeModule.OnInitializeEncounter += this.TryUpgradeEncounter;
        }

        private UpgradeEncounterStrategy? TryUpgradeEncounter(EncounterContext ctx, IWaveSelectionDefinition? waveSelectionDefinition)
        {
            if (ctx is FalseSonBossFightContext falseSonBossFightContext)
            {
                if (this.GetRank(ctx.Controller) < 4)
                {
                    return null;
                }

                return ctx.Controller.EnsureComponent<FinalBossUpgradeStrategies>().GetUpgradeStrategy(falseSonBossFightContext);
            }

            // On stage 1, if the rank is greater than zero, apply the upgrade etc.
            if (this.GetRank(ctx.Controller) <= Run.instance.stageClearCount)
            {
                return null;
            }

            if (waveSelectionDefinition != null && ctx.Controller is InfiniteTowerWaveController wave)
            {
                var upgradeStrategy = waveSelectionDefinition.GetUpgradeWaveStrategy(wave);

                if (upgradeStrategy && upgradeStrategy!.WaveUpgradeFilter == WaveUpgradeFilter.MainBoss)
                {
                    return upgradeStrategy;
                }

                return null;
            }

            if (IsMainBossWave())
            {
                this.Logger.LogError("Main boss upgrade not implemented");
            }

            return null;
        }
    }
}
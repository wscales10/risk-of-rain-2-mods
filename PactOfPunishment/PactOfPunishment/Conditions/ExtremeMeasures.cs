using PactOfPunishment.Waves.Infrastructure;
using RoR2;

namespace PactOfPunishment.Conditions
{
    public sealed class ExtremeMeasures : ConditionDef
    {
        public override int MaxRank => 4;

        public static bool IsMainBossWave()
        {
            return Run.instance is InfiniteTowerRun run && run.IsStageTransitionWave() && run.waveController.isBossWave; // TODO: this may be incorrect for stage 4+?
        }

        public override int GetHeatForRank(int rank) => rank;

        public override void Init()
        {
            WaveUpgradesModule.OnInitializeWave += this.TryUpgradeWave;
        }

        private UpgradeWaveStrategy? TryUpgradeWave(InfiniteTowerWaveController wave, IWaveSelectionDefinition? waveSelectionDefinition)
        {
            // On stage 1, if the rank is greater than zero, apply the upgrade etc.
            if (this.GetRank(wave) <= Run.instance.stageClearCount)
            {
                return null;
            }

            if (waveSelectionDefinition != null)
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
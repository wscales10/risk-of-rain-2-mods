using PactOfPunishment.Conditions;
using RoR2;

namespace PactOfPunishment.Waves.Summoner
{
    public partial class Summoner
    {
        public class SummonerUpgradeStrategy : UpgradeWaveStrategy
        {
            public override WaveUpgradeFilter WaveUpgradeFilter => WaveUpgradeFilter.MainBoss;

            public override void PostInitialise(InfiniteTowerWaveController wave)
            {
                // TODO: extreme measures 2 (once I am happy with the normal version).
            }
        }
    }
}
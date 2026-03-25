using RoR2;

namespace PactOfPunishment.Waves.Common
{
    public abstract class MainBossWaveDefinition<TWaveController> : SimulacrumWaveDefinition<TWaveController> where TWaveController : InfiniteTowerWaveController
    {
        protected override ItemTier RewardDisplayTier => ItemTier.Tier3;

        protected override void Setup(CombatDirector dir, CombatSquad squad, TWaveController wavePrefab)
        {
            base.Setup(dir, squad, wavePrefab);
            wavePrefab.secondsBeforeSuddenDeath *= 5;
            wavePrefab.suddenDeathRadiusConstrictingPerSecond /= 5f;
        }
    }
}
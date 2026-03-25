using PactOfPunishment.Conditions;
using PactOfPunishment.Waves.Common;
using RoR2;

namespace PactOfPunishment.Waves.Stage1.Halcyonites
{
    public abstract class Stage1HalcyoniteBossWaveDefinition : MainBossWaveDefinition<InfiniteTowerExplicitSpawnWaveController>
    {
        private readonly AssetPromise<CharacterSpawnCard> halcyoniteSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/DLC2/Halcyonite/cscHalcyonite.asset");

        protected override UpgradeEncounterStrategy? GetUpgradeStrategy()
        {
            return null; // TODO: Extreme measures 1
        }

        protected override PickupDropTable GetRewardDropTable(Run run)
        {
            // TODO: weight in favour of SotS items?
            return BossDropTables.Instance.GetRare(run);
        }

        protected override void Setup(CombatDirector dir, CombatSquad squad, InfiniteTowerExplicitSpawnWaveController wavePrefab)
        {
            base.Setup(dir, squad, wavePrefab);

            wavePrefab.spawnList = new InfiniteTowerExplicitSpawnWaveController.SpawnInfo[]
            {
                new InfiniteTowerExplicitSpawnWaveController.SpawnInfo
                {
                    count = 1,
                    spawnCard = this.GetHalcyoniteSpawnCard()
                }
            };
        }

        protected virtual CharacterSpawnCard GetHalcyoniteSpawnCard() => this.halcyoniteSpawnCard.Value;
    }
}
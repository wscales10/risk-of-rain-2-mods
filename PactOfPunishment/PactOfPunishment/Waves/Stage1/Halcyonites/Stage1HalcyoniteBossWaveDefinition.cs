using PactOfPunishment.Conditions;
using PactOfPunishment.Waves.Common;
using RoR2;
using UnityEngine;

namespace PactOfPunishment.Waves.Stage1.Halcyonites
{
    public abstract class Stage1HalcyoniteBossWaveDefinition : MainBossWaveDefinition<InfiniteTowerExplicitSpawnWaveController>
    {
        private readonly AssetPromise<CharacterSpawnCard> halcyoniteSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/DLC2/Halcyonite/cscHalcyonite.asset");

        protected override ItemTier RewardDisplayTier => ItemTier.Tier2;

        protected override UpgradeEncounterStrategy? GetUpgradeStrategy()
        {
            return ScriptableObject.CreateInstance<Stage1HalcyoniteUpgrade>();
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
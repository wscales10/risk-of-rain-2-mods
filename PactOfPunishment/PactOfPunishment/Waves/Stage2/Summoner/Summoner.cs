using PactOfPunishment.Conditions;
using PactOfPunishment.Waves.Common;
using PactOfPunishment.Waves.Stage2.Summoner;
using RoR2;
using UnityEngine;

namespace PactOfPunishment.Waves.Summoner
{
    public partial class Summoner : MainBossWaveDefinition<InfiniteTowerBossWaveController>
    {
        private static float mainBossCreditsFraction = 0.27f;

        protected override ItemTier RewardDisplayTier => ItemTier.Tier2;

        protected override PickupDropTable GetRewardDropTable(Run run)
        {
            return BossDropTables.Instance.GetRare(run);
        }

        protected override UpgradeEncounterStrategy GetUpgradeStrategy()
        {
            return ScriptableObject.CreateInstance<SummonerUpgradeStrategy>();
        }

        protected override void Setup(CombatDirector dir, CombatSquad squad, InfiniteTowerBossWaveController wavePrefab)
        {
            base.Setup(dir, squad, wavePrefab);

            dir.maxSquadCount = 1;
            dir.increaseSpawnDistanceOnFailure = true;
            dir.skipSpawnIfTooCheap = false;
            wavePrefab.baseCredits = defaultBossWavePrefab.Value.GetComponent<InfiniteTowerBossWaveController>().baseCredits * mainBossCreditsFraction;
            wavePrefab.immediateCreditsFraction = 1;
            wavePrefab.guaranteeInitialChampion = true;

            // TODO: boss wave start UI is not accurate? same for all bosses?
            wavePrefab.gameObject.AddComponent<SummonerBossFightBehavior>();
        }
    }
}
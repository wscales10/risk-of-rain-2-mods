using RoR2;
using System;
using UnityEngine.AddressableAssets;

namespace PactOfPunishment.Waves
{
    public abstract class MiniBossWaveDefinition<TWaveController> : SimulacrumWaveDefinition<TWaveController> where TWaveController : InfiniteTowerWaveController
    {
        private static readonly Lazy<BasicPickupDropTable> baseDropTable = new Lazy<BasicPickupDropTable>(() => Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/DLC1/GameModes/InfiniteTowerRun/ITAssets/dtITBossWave.asset").WaitForCompletion());

        protected override ItemTier RewardDisplayTier => ItemTier.Tier2;

        protected static BasicPickupDropTable GetBaseDropTable(Run run)
        {
            var output = baseDropTable.Value;
            output.RegenerateDropTable(run);
            return output;
        }

        protected static Lazy<CharacterSpawnCard> GetLazySpawnCard(string key) => new Lazy<CharacterSpawnCard>(Addressables.LoadAssetAsync<CharacterSpawnCard>(key).WaitForCompletion);

        protected override PickupDropTable GetRewardDropTable(Run run)
        {
            return GetBaseDropTable(run);
        }
    }
}
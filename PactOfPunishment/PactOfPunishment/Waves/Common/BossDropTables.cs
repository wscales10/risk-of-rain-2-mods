using RoR2;
using System;
using UnityEngine.AddressableAssets;

namespace PactOfPunishment.Waves.Common
{
    public class BossDropTables
    {
        private static BossDropTables? instance;

        private readonly Lazy<BasicPickupDropTable> legendaryDropTable = new Lazy<BasicPickupDropTable>(() => Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/DLC1/GameModes/InfiniteTowerRun/ITAssets/dtITSpecialBossWave.asset").WaitForCompletion());

        private readonly Lazy<BasicPickupDropTable> rareDropTable = new Lazy<BasicPickupDropTable>(() => Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/DLC1/GameModes/InfiniteTowerRun/ITAssets/dtITBossWave.asset").WaitForCompletion());

        private BossDropTables()
        {
        }

        public static BossDropTables Instance => instance ??= new BossDropTables();

        public BasicPickupDropTable GetLegendary(Run run)
        {
            var output = this.legendaryDropTable.Value;
            output.RegenerateDropTable(run);
            return output;
        }

        public BasicPickupDropTable GetRare(Run run)
        {
            var output = this.rareDropTable.Value;
            output.RegenerateDropTable(run);
            return output;
        }
    }
}
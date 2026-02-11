using PactOfPunishment.Conditions;
using RoR2;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static RoR2.InfiniteTowerExplicitSpawnWaveController;

namespace PactOfPunishment.Waves
{
    public class RunaldAndKjaro : MiniBossWaveDefinition<InfiniteTowerExplicitSpawnWaveController>
    {
        private static readonly Lazy<CharacterSpawnCard> elderLemurianSpawnCard = new Lazy<CharacterSpawnCard>(Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/LemurianBruiser/cscLemurianBruiser.asset").WaitForCompletion);

        protected override PickupDropTable GetRewardDropTable(Run run)
        {
            var baseDropTable = GetBaseDropTable(run);
            var dropTable = ScriptableObject.CreateInstance<BetterExplicitPickupDropTable>();
            int count = baseDropTable.GetPickupCount();
            float totalRingWeight = 0, totalWeight = baseDropTable.selector.getTotalWeight();
            dropTable.pickupEntries = new BetterExplicitPickupDropTable.PickupIndexEntry[count];

            for (int i = 0; i < count; i++)
            {
                var current = baseDropTable.selector.GetChoice(i);

                if (current.value.pickupIndex == PickupCatalog.FindPickupIndex(RoR2Content.Items.FireRing.itemIndex) || current.value.pickupIndex == PickupCatalog.FindPickupIndex(RoR2Content.Items.IceRing.itemIndex))
                {
                    totalRingWeight += current.weight;
                }
            }

            float desiredRingChance = 2f / 4;
            float ringWeightMultiplier = (desiredRingChance / (1 - desiredRingChance)) * (totalWeight - totalRingWeight) / (totalRingWeight);

            for (int i = 0; i < count; i++)
            {
                var current = baseDropTable.selector.GetChoice(i);
                float adjustedWeight = current.weight;

                if (current.value.pickupIndex == PickupCatalog.FindPickupIndex(RoR2Content.Items.FireRing.itemIndex) || current.value.pickupIndex == PickupCatalog.FindPickupIndex(RoR2Content.Items.IceRing.itemIndex))
                {
                    adjustedWeight *= ringWeightMultiplier;
                }

                dropTable.pickupEntries[i] = new BetterExplicitPickupDropTable.PickupIndexEntry { pickupIndex = current.value.pickupIndex, pickupWeight = adjustedWeight };
            }

            return dropTable; // TODO: consider maintaining rarity distribution
        }

        protected override UpgradeWaveStrategy GetUpgradeStrategy() => ScriptableObject.CreateInstance<ReplaceWithFourEliteElders>();

        protected override void Setup(CombatDirector dir, CombatSquad squad, InfiniteTowerExplicitSpawnWaveController wavePrefab)
        {
            base.Setup(dir, squad, wavePrefab);
            wavePrefab.spawnList = new SpawnInfo[]
            {
                new SpawnInfo
                {
                    count = 1,
                    eliteDef = RoR2Content.Elites.Ice,
                    spawnCard = elderLemurianSpawnCard.Value,
                },
                new SpawnInfo
                {
                    count = 1,
                    eliteDef = RoR2Content.Elites.Fire,
                    spawnCard = elderLemurianSpawnCard.Value,
                },
            };
        }

        public class ReplaceWithFourEliteElders : UpgradeWaveStrategy
        {
            public override void UpgradeWave(InfiniteTowerWaveController wave)
            {
                var eliteDefs = Utils.GetEliteDefs(elderLemurianSpawnCard.Value).ToArray();
                Util.ShuffleArray(eliteDefs, wave.rng);
                ((InfiniteTowerExplicitSpawnWaveController)wave).spawnList = eliteDefs.Take(4).Select(x => new SpawnInfo
                {
                    count = 1,
                    eliteDef = x,
                    spawnCard = elderLemurianSpawnCard.Value,
                }).ToArray();
            }
        }
    }
}
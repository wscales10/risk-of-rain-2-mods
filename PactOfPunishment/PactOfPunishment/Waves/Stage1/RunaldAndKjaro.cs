using PactOfPunishment.Conditions;
using PactOfPunishment.Waves.Common;
using RoR2;
using System;
using System.Linq;
using UnityEngine;
using static RoR2.InfiniteTowerExplicitSpawnWaveController;

namespace PactOfPunishment.Waves.Stage1
{
    public class RunaldAndKjaro : MiniBossWaveDefinition<InfiniteTowerExplicitSpawnWaveController>
    {
        private readonly AssetPromise<CharacterSpawnCard> elderLemurianSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/Base/LemurianBruiser/cscLemurianBruiser.asset");

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
            float ringWeightMultiplier = desiredRingChance / (1 - desiredRingChance) * (totalWeight - totalRingWeight) / totalRingWeight;

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
                    spawnCard = this.elderLemurianSpawnCard.Value,
                },
                new SpawnInfo
                {
                    count = 1,
                    spawnCard = this.elderLemurianSpawnCard.Value,
                },
            };
            wavePrefab.gameObject.AddComponent<ElderLemuriansBehavior>().eliteEquipments = new EquipmentDef[] { RoR2Content.Equipment.AffixWhite, RoR2Content.Equipment.AffixRed };
        }

        public class ElderLemuriansBehavior : BossFightBehavior
        {
            public EquipmentDef[] eliteEquipments = Array.Empty<EquipmentDef>();

            private int spawnIndex = 0;

            protected override void OnBossSpawnedServer(CharacterBody body)
            {
                body.ScaleDifficultyAsBoss(0.86f, 40, true, false);

                if (this.eliteEquipments.Length > 0)
                {
                    body.inventory.SetEquipmentIndex(this.eliteEquipments[this.spawnIndex].equipmentIndex, false);
                    this.spawnIndex = (this.spawnIndex + 1) % this.eliteEquipments.Length;
                }
                else
                {
                    Debug.LogWarning("eliteEquipments.Length is 0");
                }
            }
        }

        public class ReplaceWithFourEliteElders : UpgradeWaveStrategy
        {
            private readonly AssetPromise<CharacterSpawnCard> elderLemurianSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/Base/LemurianBruiser/cscLemurianBruiser.asset");

            public override WaveUpgradeFilter WaveUpgradeFilter => WaveUpgradeFilter.MiniBoss;

            public override void PostInitialise(InfiniteTowerWaveController wave)
            {
                ((InfiniteTowerExplicitSpawnWaveController)wave).spawnList = Enumerable.Range(0, 4).Select(x => new SpawnInfo
                {
                    count = 1,
                    spawnCard = this.elderLemurianSpawnCard.Value,
                }).ToArray();

                var eliteDefs = Utils.GetEliteDefs(this.elderLemurianSpawnCard.Value).Select(x => x.eliteEquipmentDef).Distinct().ToArray();
                Util.ShuffleArray(eliteDefs, wave.rng);

                foreach (var eliteDef in eliteDefs)
                {
                    Debug.Log($"Possible elite type for Elder Lemurian boss: '{eliteDef.name}'");
                }

                wave.GetComponent<ElderLemuriansBehavior>().eliteEquipments = eliteDefs.Take(4).ToArray();
            }
        }
    }
}
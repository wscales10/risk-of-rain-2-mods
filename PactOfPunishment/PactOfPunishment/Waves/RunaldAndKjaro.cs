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

        protected override UpgradeWaveStrategy GetUpgradeMiniBossStrategy() => ScriptableObject.CreateInstance<ReplaceWithFourEliteElders>();

        protected override void Setup(CombatDirector dir, CombatSquad squad, InfiniteTowerExplicitSpawnWaveController wavePrefab)
        {
            base.Setup(dir, squad, wavePrefab);
            wavePrefab.spawnList = new SpawnInfo[]
            {
                new SpawnInfo
                {
                    count = 1,
                    spawnCard = elderLemurianSpawnCard.Value,
                },
                new SpawnInfo
                {
                    count = 1,
                    spawnCard = elderLemurianSpawnCard.Value,
                },
            };
            wavePrefab.gameObject.AddComponent<ElderLemuriansBehavior>().eliteEquipments = new EquipmentDef[] { RoR2Content.Equipment.AffixWhite, RoR2Content.Equipment.AffixRed };
        }

        public class ElderLemuriansBehavior : MonoBehaviour
        {
            public EquipmentDef[] eliteEquipments = Array.Empty<EquipmentDef>();

            private int spawnIndex = 0;

            public void Awake()
            {
                var combatDirector = this.GetComponent<CombatDirector>();
                combatDirector.onSpawnedServer ??= new CombatDirector.OnSpawnedServer();
                combatDirector.onSpawnedServer.AddListener(this.OnBossSpawnedServer);
            }

            private void OnBossSpawnedServer(GameObject spawnedEntity)
            {
                var body = Utils.GetCharacterBody(spawnedEntity);

                if (body)
                {
                    body!.master.ScaleDifficultyAsBoss(0.77f, 6, true); // TODO: do something similar for other bosses

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
                else
                {
                    Debug.LogWarning("eliteEquipments.Length is 0");
                }
            }
        }

        public class ReplaceWithFourEliteElders : UpgradeWaveStrategy
        {
            public override void UpgradeWave(InfiniteTowerWaveController wave)
            {
                ((InfiniteTowerExplicitSpawnWaveController)wave).spawnList = Enumerable.Range(0, 4).Select(x => new SpawnInfo
                {
                    count = 1,
                    spawnCard = elderLemurianSpawnCard.Value,
                }).ToArray();

                var eliteDefs = Utils.GetEliteDefs(elderLemurianSpawnCard.Value).Select(x => x.eliteEquipmentDef).Distinct().ToArray();
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
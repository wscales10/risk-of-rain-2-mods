using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AssortedExperiments
{
    internal static class PickupDropTableUtils
    {
        private const float verySmallFloat = 1e-5f;

        public static IReadOnlyDictionary<bool, WeightedSelection<T>> FilterSelection<T>(WeightedSelection<T> original, Func<T, bool> filter)
        {
            var output = new Dictionary<bool, WeightedSelection<T>>
            {
                [false] = new WeightedSelection<T>(original.Capacity),
                [true] = new WeightedSelection<T>(original.Capacity)
            };

            foreach (var choice in original.choices)
            {
                if (choice.weight < verySmallFloat)
                {
                    continue;
                }

                output[filter(choice.value)].AddChoice(choice);
            }

            return output;
        }

        public static Rarity GetRarity(PickupDef pickupDef)
        {
            if (pickupDef.droneIndex != DroneIndex.None)
            {
                return new DroneRarity(DroneCatalog.GetDroneDef(pickupDef.droneIndex).tier);
            }
            else if (pickupDef.equipmentIndex != EquipmentIndex.None)
            {
                return new EquipmentRarity(EquipmentCatalog.GetEquipmentDef(pickupDef.equipmentIndex).isLunar);
            }
            else if (pickupDef.itemIndex != ItemIndex.None)
            {
                return new ItemRarity(ItemCatalog.GetItemDef(pickupDef.itemIndex).tier);
            }
            else
            {
                return new ItemTierRarity(pickupDef.itemTier);
            }
        }

        public static WeightedSelection<UniquePickup> GetWeightedSelection(PickupDropTable table)
        {
            // TODO: Allow other mods to extend this capability.
            switch (table)
            {
                case ArenaMonsterItemDropTable arenaTable:
                    arenaTable.GenerateWeightedSelection(Run.instance);
                    return arenaTable.selector;

                case BasicPickupDropTable basicTable:
                    return basicTable.selector;

                case DoppelgangerDropTable doppelgangerTable:
                    return doppelgangerTable.selector;

                case ExplicitPickupDropTable explicitTable:
                    return explicitTable.weightedSelection;

                case FreeChestDropTable freeChestTable:
                    freeChestTable.RebuildSelection();
                    return freeChestTable.selector;

                case TripleDroneShopDropTable tripleDroneTable:
                    return tripleDroneTable.weightedSelection;

                default:
                    throw new NotImplementedException($"Getting WeightedSelection from {table.GetType().Name} is not implemented.");
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="original"></param>
        /// <param name="filter"></param>
        /// <param name="newItemProbability">
        /// Probability of picking an item the player doesn't already have.
        /// </param>
        /// <returns></returns>
        public static WeightedSelection<UniquePickup> IncreaseWeights(WeightedSelection<UniquePickup> original, Func<UniquePickup, bool> filter, UnownedItemProbabilityGetter getUnownedItemProbability, float adjustmentStrength)
        {
            var output = new WeightedSelection<UniquePickup>(original.Capacity);

            foreach (var grouping in original.choices.GroupBy(choice => GetRarity(PickupCatalog.GetPickupDef(choice.value.pickupIndex)), RarityEqualityComparer.Instance))
            {
                bool success = TryIncreaseWeightsForRarity(filter, getUnownedItemProbability, adjustmentStrength, output, grouping.Key, grouping.ToArray());

                if (!success)
                {
                    foreach (var choice in grouping)
                    {
                        output.AddChoice(choice);
                    }
                }
            }

            return output;
        }

        public static bool ShouldTryStackRarity(Rarity rarity)
        {
            // Leave equipment pickups unchanged
            if (rarity is EquipmentRarity)
            {
                return false;
            }

            switch (rarity.Tier)
            {
                // Leave legendary and boss tier pickups unchanged
                case ItemTier.Tier3:
                case ItemTier.VoidTier3:
                case ItemTier.Boss:
                case ItemTier.VoidBoss:
                    return false;

                default:
                    return true;
            }
        }

        private static bool TryIncreaseWeightsForRarity(Func<UniquePickup, bool> filter, UnownedItemProbabilityGetter getUnownedItemProbability, float adjustmentStrength, WeightedSelection<UniquePickup> output, Rarity rarity, IReadOnlyCollection<WeightedSelection<UniquePickup>.ChoiceInfo> choicesForRarity)
        {
            if (!ShouldTryStackRarity(rarity))
            {
                return false;
            }

            float totalOriginalWeightForRarity = choicesForRarity.Sum(choice => choice.weight);

            if (totalOriginalWeightForRarity < verySmallFloat)
            {
                return false;
            }

            var matchingArray = choicesForRarity.Where(choice => filter(choice.value)).ToArray();

            if (matchingArray.Length == 0)
            {
                return false;
            }

            var totalMatchingWeight = matchingArray.Sum(choice => choice.weight);

            if (totalMatchingWeight < verySmallFloat)
            {
                return false;
            }

            float newItemProbability = getUnownedItemProbability(matchingArray.Length, choicesForRarity.Count, adjustmentStrength);
            newItemProbability = Math.Clamp(newItemProbability, 0, 1);
            var matchingWeightProportion = totalMatchingWeight / totalOriginalWeightForRarity;
            var commonFactor = 1 / (1 - newItemProbability * matchingWeightProportion);

            foreach (var choice in choicesForRarity)
            {
                if (matchingArray.Contains(choice))
                {
                    output.AddChoice(new WeightedSelection<UniquePickup>.ChoiceInfo { value = choice.value, weight = choice.weight * commonFactor * (1 - newItemProbability) / matchingWeightProportion });
                }
                else
                {
                    output.AddChoice(new WeightedSelection<UniquePickup>.ChoiceInfo { value = choice.value, weight = choice.weight * commonFactor * newItemProbability });
                }
            }

            return true;
        }
    }
}
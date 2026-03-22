using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PactOfPunishment
{
    public class BetterExplicitPickupDropTable : PickupDropTable
    {
        public PickupIndexEntry[] pickupEntries = Array.Empty<PickupIndexEntry>();

        private readonly WeightedSelection<UniquePickup> weightedSelection = new WeightedSelection<UniquePickup>();

        public static BetterExplicitPickupDropTable ReplaceTierWithSingleItem(BasicPickupDropTable baseDropTable, ItemDef item)
        {
            var dropTable = CreateInstance<BetterExplicitPickupDropTable>();
            int count = baseDropTable.GetPickupCount();
            float totalWeightForMyItemTier = 0, totalWeightForMyItem = 0;
            var pickupEntries = new List<PickupIndexEntry>();

            for (int i = 0; i < count; i++)
            {
                var current = baseDropTable.selector.GetChoice(i);
                var pickupDef = PickupCatalog.GetPickupDef(current.value.pickupIndex);

                if (pickupDef.itemTier == item.tier)
                {
                    totalWeightForMyItemTier += current.weight;
                }

                if (pickupDef.itemIndex == item.itemIndex)
                {
                    totalWeightForMyItem += current.weight;
                }
            }

            bool foundMyItem = !Mathf.Approximately(totalWeightForMyItem, 0);
            bool isItemAvailable = Run.instance.IsItemAvailable(item.itemIndex);

            for (int i = 0; i < count; i++)
            {
                var current = baseDropTable.selector.GetChoice(i);
                var pickupDef = PickupCatalog.GetPickupDef(current.value.pickupIndex);
                float adjustedWeight = current.weight;

                if (pickupDef.itemIndex == item.itemIndex)
                {
                    if (foundMyItem)
                    {
                        adjustedWeight *= totalWeightForMyItemTier / totalWeightForMyItem;
                    }
                    else
                    {
                        continue;
                    }
                }
                else if (pickupDef.itemTier == item.tier && (foundMyItem || isItemAvailable))
                {
                    continue;
                }

                pickupEntries.Add(new PickupIndexEntry { pickupIndex = pickupDef.pickupIndex, pickupWeight = adjustedWeight });
            }

            if (!foundMyItem && isItemAvailable)
            {
                pickupEntries.Add(new PickupIndexEntry { pickupIndex = PickupCatalog.FindPickupIndex(item.itemIndex), pickupWeight = totalWeightForMyItemTier });
            }

            dropTable.pickupEntries = pickupEntries.ToArray();
            return dropTable;
        }

        public override void Regenerate(Run run)
        {
            this.GenerateWeightedSelection();
        }

        public override UniquePickup GeneratePickupPreReplacement(Xoroshiro128Plus rng)
        {
            this.Regenerate(Run.instance); // TODO: see if you can remove these
            return GeneratePickupFromWeightedSelection(rng, this.weightedSelection);
        }

        public override void GenerateDistinctPickupsPreReplacement(List<UniquePickup> dest, int desiredCount, Xoroshiro128Plus rng)
        {
            this.Regenerate(Run.instance);
            GenerateDistinctFromWeightedSelection(dest, desiredCount, rng, this.weightedSelection);
        }

        public override int GetPickupCount()
        {
            return this.weightedSelection.Count;
        }

        private void GenerateWeightedSelection()
        {
            this.weightedSelection.Clear();

            PickupIndexEntry[] array2 = this.pickupEntries;
            for (int i = 0; i < array2.Length; i++)
            {
                PickupIndexEntry pickupDefEntry = array2[i];
                PickupIndex pickupIndex = pickupDefEntry.pickupIndex;
                if (pickupIndex != PickupIndex.none)
                {
                    this.weightedSelection.AddChoice(new UniquePickup(pickupIndex), pickupDefEntry.pickupWeight);
                }
            }
        }

        [Serializable]
        public struct PickupIndexEntry
        {
            public PickupIndex pickupIndex;

            public float pickupWeight;
        }
    }
}
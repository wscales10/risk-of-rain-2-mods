using RoR2;
using System;
using System.Collections.Generic;

namespace PactOfPunishment
{
    public class BetterExplicitPickupDropTable : PickupDropTable
    {
        [Serializable]
        public struct PickupIndexEntry
        {
            public PickupIndex pickupIndex;

            public float pickupWeight;
        }

        public PickupIndexEntry[] pickupEntries = Array.Empty<PickupIndexEntry>();

        private readonly WeightedSelection<UniquePickup> weightedSelection = new WeightedSelection<UniquePickup>();

        public override void Regenerate(Run run)
        {
            this.GenerateWeightedSelection();
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
    }
}
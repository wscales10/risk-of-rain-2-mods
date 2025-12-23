using RoR2;
using System;
using System.Collections.Generic;

namespace AssortedExperiments
{
    internal class TemporaryPickupDropTable : PickupDropTable
    {
        public WeightedSelection<UniquePickup>? Selector { get; set; }

        public override int GetPickupCount()
        {
            throw new NotImplementedException();
        }

        public override UniquePickup GeneratePickupPreReplacement(Xoroshiro128Plus rng)
        {
            return GeneratePickupFromWeightedSelection(rng, this.Selector);
        }

        public override void GenerateDistinctPickupsPreReplacement(List<UniquePickup> dest, int desiredCount, Xoroshiro128Plus rng)
        {
            GenerateDistinctFromWeightedSelection(dest, desiredCount, rng, this.Selector);
        }
    }
}
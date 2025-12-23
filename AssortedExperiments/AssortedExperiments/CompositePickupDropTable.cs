using BepInEx.Logging;
using RoR2;
using System.Collections.Generic;
using System.Linq;

namespace AssortedExperiments
{
    public class CompositePickupDropTable : PickupDropTable
    {
        private WeightedSelection<UniquePickup>? preferredSelector;

        private WeightedSelection<UniquePickup>? complementSelector;

        public ManualLogSource? Logger { get; set; }

        public WeightedSelection<UniquePickup>? OriginalSelector { get; set; }

        public void SetFilterSelectionResult(IReadOnlyDictionary<bool, WeightedSelection<UniquePickup>> filterSelectionResult)
        {
            this.preferredSelector = filterSelectionResult[true];
            this.complementSelector = filterSelectionResult[false];
        }

        public override int GetPickupCount()
        {
            return this.OriginalSelector?.Count ?? 0;
        }

        public override UniquePickup GeneratePickupPreReplacement(Xoroshiro128Plus rng)
        {
            var original = GeneratePickupFromWeightedSelection(rng, this.OriginalSelector);
            var originalPickupDef = PickupCatalog.GetPickupDef(original.pickupIndex);
            var originalRarity = PickupDropTableUtils.GetRarity(originalPickupDef);

            if (!PickupDropTableUtils.ShouldTryStackRarity(originalRarity))
            {
                return original;
            }

            var preferredSelectorFilteredByRarity = PickupDropTableUtils.FilterSelection(this.preferredSelector!, pickup => RarityEqualityComparer.Instance.Equals(originalRarity, PickupDropTableUtils.GetRarity(PickupCatalog.GetPickupDef(pickup.pickupIndex))))[true];

            if (preferredSelectorFilteredByRarity.Count > 0)
            {
                UniquePickup replacementPickup = GeneratePickupFromWeightedSelection(rng, preferredSelectorFilteredByRarity);

                // Note: this can sometimes replace preferred pickups with themselves or other preferred pickups, but I think that's acceptable.
                this.Logger?.LogDebug($"Replacing {Language.GetString(originalPickupDef.nameToken)} with {Language.GetString(PickupCatalog.GetPickupDef(replacementPickup.pickupIndex).nameToken)}.");
                return replacementPickup;
            }
            else
            {
                this.Logger?.LogDebug($"No preferred pickups available to replace {Language.GetString(originalPickupDef.nameToken)}. Keeping original.");
                return original;
            }
        }

        public override void GenerateDistinctPickupsPreReplacement(List<UniquePickup> dest, int desiredCount, Xoroshiro128Plus rng)
        {
            var original = GenerateDistinctFromWeightedSelection(new List<UniquePickup>(), desiredCount, rng, this.OriginalSelector).Select((pickup, index) => (pickup, index, rarity: PickupDropTableUtils.GetRarity(PickupCatalog.GetPickupDef(pickup.pickupIndex)))).ToArray();
            var output = new List<UniquePickup?>(desiredCount);

            foreach (var grouping in original.GroupBy(x => x.rarity, RarityEqualityComparer.Instance))
            {
                var tempList = this.GenerateDistinctPickupsForRarityGroup(grouping.Key, grouping.Select(x => x.pickup).ToList(), rng);

                int i = 0;

                foreach (var (_, index, _) in grouping)
                {
                    output[index] = tempList[i];
                    i++;
                }
            }

            dest.AddRange(output.Where(x => x.HasValue).Select(x => x!.Value));
            this.Logger?.LogDebug($"Replacing [{string.Join(", ", original.Select(x => Language.GetString(PickupCatalog.GetPickupDef(x.pickup.pickupIndex).nameToken)))}] with [{string.Join(", ", dest.Select(x => Language.GetString(PickupCatalog.GetPickupDef(x.pickupIndex).nameToken)))}].");
        }

        private IReadOnlyList<UniquePickup> GenerateDistinctPickupsForRarityGroup(Rarity rarity, IReadOnlyList<UniquePickup> originalList, Xoroshiro128Plus rng)
        {
            if (!PickupDropTableUtils.ShouldTryStackRarity(rarity))
            {
                return originalList;
            }

            int desiredCount = originalList.Count;

            var preferredSelectorFilteredByRarity = PickupDropTableUtils.FilterSelection(this.preferredSelector!, pickup => RarityEqualityComparer.Instance.Equals(rarity, PickupDropTableUtils.GetRarity(PickupCatalog.GetPickupDef(pickup.pickupIndex))))[true];

            int remaining = desiredCount - preferredSelectorFilteredByRarity.Count;

            var tempList = GenerateDistinctFromWeightedSelection(new List<UniquePickup>(), desiredCount, rng, preferredSelectorFilteredByRarity);

            if (remaining > 0)
            {
                var complementSelectorFilteredByRarity = PickupDropTableUtils.FilterSelection(this.complementSelector!, pickup => RarityEqualityComparer.Instance.Equals(rarity, PickupDropTableUtils.GetRarity(PickupCatalog.GetPickupDef(pickup.pickupIndex))))[true];
                tempList.AddRange(GenerateDistinctFromWeightedSelection(new List<UniquePickup>(), remaining, rng, complementSelectorFilteredByRarity));
            }

            return tempList;
        }
    }
}
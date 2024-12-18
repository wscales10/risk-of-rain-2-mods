using RoR2;

namespace UntitledMod
{
    public class InventoriesInfo
    {
        private readonly IInventoryManagers inventoryManagers;

        private readonly IPickupWeightMultipliers pickupWeightMultipliers;

        public InventoriesInfo(IInventoryManagers inventoryManagers, IPickupWeightMultipliers pickupWeightMultipliers)
        {
            this.inventoryManagers = inventoryManagers;
            this.pickupWeightMultipliers = pickupWeightMultipliers;
        }

        public bool Lookup(CharacterMaster characterMaster, out IReadOnlyInventoryManager inventoryManager)
        {
            if (this.inventoryManagers.TryGetValue(characterMaster, out var output))
            {
                inventoryManager = output;
                return true;
            }
            else
            {
                inventoryManager = null;
                return false;
            }
        }

        public float GetPickupWeightMultiplier(PickupIndex pickupIndex)
        {
            if (this.pickupWeightMultipliers.TryGetValue(pickupIndex, out float multiplier))
            {
                return multiplier;
            }
            else
            {
                return 1;
            }
        }
    }
}
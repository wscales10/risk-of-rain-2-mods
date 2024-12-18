using RoR2;
using System;
using System.Linq;

namespace UntitledMod
{
    public class Writer
    {
        private readonly IInventoryManagers inventoryManagers;

        private readonly IPickupWeightMultipliers pickupWeightMultipliers;

        private readonly Func<ItemIndex, PickupIndex> findPickupIndex;

        private readonly ICustomLogger logger;

        private readonly ServerSide serverSide;

        public Writer(ICustomLogger logger, IInventoryManagers inventoryManagers, ServerSide serverSide, IPickupWeightMultipliers pickupWeightMultipliers, Func<ItemIndex, PickupIndex> findPickupIndex)
        {
            this.logger = logger;
            this.inventoryManagers = inventoryManagers;
            this.serverSide = serverSide;
            this.pickupWeightMultipliers = pickupWeightMultipliers;
            this.findPickupIndex = findPickupIndex;
        }

        public void TryAddInventoryManager(PlayerCharacterMasterController self)
        {
            this.serverSide.TryExecute(() => this.inventoryManagers.Add(self.master));
        }

        public void OnPickupItem(Inventory inventory, ItemIndex itemIndex)
        {
            this.serverSide.TryExecute(() =>
            {
                if (this.inventoryManagers.TryGetValue(inventory, out var inventoryManager))
                {
                    inventoryManager.OnPickupItem(itemIndex);
                    this.RefreshItemWeightMultiplier(itemIndex);
                }
            });
        }

        public void OnLoseItem(Inventory inventory, ItemIndex itemIndex)
        {
            if (this.inventoryManagers.TryGetValue(inventory, out var inventoryManager))
            {
                inventoryManager.OnLoseItem(itemIndex);
                this.RefreshItemWeightMultiplier(itemIndex);
            }
        }

        public void Reset()
        {
            this.inventoryManagers.Reset();
            this.pickupWeightMultipliers.Reset();
        }

        public void OnRemoveItem(Inventory inventory, ItemIndex itemIndex)
        {
            if (inventory.itemStacks[(int)itemIndex] == 0)
            {
                this.serverSide.TryExecute(() => this.OnLoseItem(inventory, itemIndex));
            }
        }

        public void RefreshItemWeightMultiplier(ItemIndex itemIndex)
        {
            var pickupIndex = this.findPickupIndex(itemIndex);
            int bannedFor = this.inventoryManagers.Count(inventoryManager => !inventoryManager.IsAllowed(itemIndex));

            if (bannedFor == 0)
            {
                this.pickupWeightMultipliers.SetValue(pickupIndex, null);
            }
            else
            {
                this.pickupWeightMultipliers.SetValue(pickupIndex, Math.Clamp(1 - bannedFor / (float)this.inventoryManagers.Count(), 0, 1));
            }

            typeof(Run).RaiseStaticEvent(nameof(Run.onAvailablePickupsModified));
        }
    }
}
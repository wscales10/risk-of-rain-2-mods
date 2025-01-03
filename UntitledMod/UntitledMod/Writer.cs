using RoR2;
using System;
using System.Collections.Specialized;
using System.Linq;
using UntitledMod.Context;

namespace UntitledMod
{
    public class Writer
    {
        private readonly IInventoryManagers inventoryManagers;

        private readonly IRoR2Context gameContext;

        private readonly IPickupWeightMultipliers pickupWeightMultipliers;

        private readonly Func<ItemIndex, PickupIndex> findPickupIndex;

        private readonly ICustomLogger logger;

        public Writer(ICustomLogger logger, IInventoryManagers inventoryManagers, IRoR2Context gameContext, IPickupWeightMultipliers pickupWeightMultipliers, Func<ItemIndex, PickupIndex> findPickupIndex)
        {
            this.logger = logger;
            this.inventoryManagers = inventoryManagers;
            this.gameContext = gameContext;
            this.pickupWeightMultipliers = pickupWeightMultipliers;
            this.findPickupIndex = findPickupIndex;
        }

        public event Action ItemWeightMultipliersUpdated;

        public void TryAddInventoryManager(PlayerCharacterMasterController self)
        {
            this.inventoryManagers.Add(self).BannedItemsChanged += this.Writer_BannedItemsChanged;
        }

        public void OnPickupItem(Inventory inventory, ItemIndex itemIndex)
        {
            this.gameContext.ThrowIfClient();
            if (this.inventoryManagers.TryGetValue(inventory, out var inventoryManager))
            {
                inventoryManager.OnPickupItem(itemIndex);
            }
        }

        public void OnLoseItem(Inventory inventory, ItemIndex itemIndex)
        {
            if (this.inventoryManagers.TryGetValue(inventory, out var inventoryManager))
            {
                inventoryManager.OnLoseItem(itemIndex);
            }
        }

        public void Reset()
        {
            this.inventoryManagers.Reset();
            this.pickupWeightMultipliers.Reset();
        }

        public void OnRemoveItem(Inventory inventory, ItemIndex itemIndex)
        {
            this.gameContext.ThrowIfClient();
            if (inventory.itemStacks[(int)itemIndex] == 0)
            {
                this.OnLoseItem(inventory, itemIndex);
            }
        }

        public void RefreshItemWeightMultipliers(params ItemIndex[] itemIndices)
        {
            bool wasUpdated = false;

            foreach (var itemIndex in itemIndices)
            {
                wasUpdated |= this.RefreshItemWeightMultiplier(itemIndex);
            }

            if (wasUpdated)
            {
                this.ItemWeightMultipliersUpdated?.Invoke();
            }
        }

        private void Writer_BannedItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.gameContext.IsNetworkServerActive)
            {
                if (e.OldItems != null)
                {
                    this.RefreshItemWeightMultipliers(e.OldItems.Cast<ItemIndex>().ToArray());
                }

                if (e.NewItems != null)
                {
                    this.RefreshItemWeightMultipliers(e.NewItems.Cast<ItemIndex>().ToArray());
                }
            }
        }

        private bool RefreshItemWeightMultiplier(ItemIndex itemIndex)
        {
            var pickupIndex = this.findPickupIndex(itemIndex);
            int bannedFor = this.inventoryManagers.Count(inventoryManager => !inventoryManager.IsAllowed(itemIndex));

            if (bannedFor == 0)
            {
                return this.pickupWeightMultipliers.SetValue(pickupIndex, null);
            }
            else
            {
                return this.pickupWeightMultipliers.SetValue(pickupIndex, Math.Clamp(1 - bannedFor / (float)this.inventoryManagers.Count(), 0, 1));
            }
        }
    }
}
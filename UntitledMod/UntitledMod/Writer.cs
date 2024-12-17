using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UntitledMod
{
    public class Writer
    {
        private readonly Dictionary<CharacterMaster, IInventoryManager> inventoryManagers = new Dictionary<CharacterMaster, IInventoryManager>();

        private readonly CustomLogger logger;

        private readonly Func<IInventoryManager> inventoryManagerFactory;

        private readonly ServerSide serverSide;

        public Writer(CustomLogger logger, Func<IInventoryManager> inventoryManagerFactory, ServerSide serverSide)
        {
            this.logger = logger;
            this.inventoryManagerFactory = inventoryManagerFactory;
            this.serverSide = serverSide;

            On.RoR2.Run.Start += this.Run_Start;
            On.RoR2.PlayerCharacterMasterController.Awake += this.PlayerCharacterMasterController_Awake;
            On.RoR2.Inventory.GiveItem_ItemIndex_int += this.Inventory_GiveItem_ItemIndex_int;
            On.RoR2.Inventory.RemoveItem_ItemIndex_int += this.Inventory_RemoveItem_ItemIndex_int;
        }

        internal bool TryGetInventoryManager(CharacterMaster characterMaster, out IInventoryManager inventoryManager)
        {
            this.logger.LogMethodCall();
            if (characterMaster is null)
            {
                inventoryManager = null;
                return false;
            }

            return this.inventoryManagers.TryGetValue(characterMaster, out inventoryManager);
        }

        private bool TryGetInventoryManager(Inventory inventory, out IInventoryManager inventoryManager)
        {
            var characterMaster = this.inventoryManagers.Keys.SingleOrDefault(m => m.inventory == inventory);
            return this.TryGetInventoryManager(characterMaster, out inventoryManager);
        }

        private void OnPickupItem(Inventory inventory, ItemIndex itemIndex)
        {
            if (this.TryGetInventoryManager(inventory, out var inventoryManager))
            {
                inventoryManager.OnPickupItem(itemIndex);
            }
        }

        private void OnLoseItem(Inventory inventory, ItemIndex itemIndex)
        {
            if (this.TryGetInventoryManager(inventory, out var inventoryManager))
            {
                inventoryManager.OnLoseItem(itemIndex);
            }
        }

        private void PlayerCharacterMasterController_Awake(On.RoR2.PlayerCharacterMasterController.orig_Awake orig, PlayerCharacterMasterController self)
        {
            this.logger.LogMethodCall();
            orig(self);
            this.serverSide.TryExecute(() => this.inventoryManagers.Add(self.master, this.inventoryManagerFactory()));
        }

        private void Run_Start(On.RoR2.Run.orig_Start orig, Run self)
        {
            this.logger.LogMethodCall();
            this.inventoryManagers.Clear();
            orig(self);
        }

        private void Inventory_GiveItem_ItemIndex_int(On.RoR2.Inventory.orig_GiveItem_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int count)
        {
            this.serverSide.TryExecute(() => this.OnPickupItem(self, itemIndex));
            orig(self, itemIndex, count);
        }

        private void Inventory_RemoveItem_ItemIndex_int(On.RoR2.Inventory.orig_RemoveItem_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int count)
        {
            try
            {
                orig(self, itemIndex, count);
            }
            finally
            {
                if (self.itemStacks[(int)itemIndex] == 0)
                {
                    this.serverSide.TryExecute(() => this.OnLoseItem(self, itemIndex));
                }
            }
        }
    }
}
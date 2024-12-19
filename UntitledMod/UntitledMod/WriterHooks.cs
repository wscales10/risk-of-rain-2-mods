using RoR2;
using System;

namespace UntitledMod
{
    public class WriterHooks
    {
        private readonly ICustomLogger logger;

        private readonly Writer writer;

        public WriterHooks(ICustomLogger logger, Writer writer)
        {
            On.RoR2.Run.Start += this.Run_Start;
            On.RoR2.PlayerCharacterMasterController.Awake += this.PlayerCharacterMasterController_Awake;
            On.RoR2.Inventory.GiveItem_ItemIndex_int += this.Inventory_GiveItem_ItemIndex_int;
            On.RoR2.Inventory.RemoveItem_ItemIndex_int += this.Inventory_RemoveItem_ItemIndex_int;
            this.logger = logger;
            this.writer = writer;

            this.writer.ItemWeightMultipliersUpdated += this.Writer_ItemWeightMultipliersUpdated;
        }

        private void Writer_ItemWeightMultipliersUpdated()
        {
            typeof(Run).GetPrivateStaticFieldValue<Action<Run>>("onAvailablePickupsModified")(Run.instance);
        }

        private void PlayerCharacterMasterController_Awake(On.RoR2.PlayerCharacterMasterController.orig_Awake orig, PlayerCharacterMasterController self)
        {
            this.logger.LogMethodCall();
            orig(self);
            this.writer.TryAddInventoryManager(self);
        }

        private void Run_Start(On.RoR2.Run.orig_Start orig, Run self)
        {
            this.logger.LogMethodCall();
            this.writer.Reset();
            orig(self);
        }

        private void Inventory_GiveItem_ItemIndex_int(On.RoR2.Inventory.orig_GiveItem_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int count)
        {
            this.writer.OnPickupItem(self, itemIndex);
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
                this.writer.OnRemoveItem(self, itemIndex);
            }
        }
    }
}
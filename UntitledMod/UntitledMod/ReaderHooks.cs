using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using System.Collections.Generic;
using System.Linq;

namespace UntitledMod
{
    public partial class ReaderHooks
    {
        private readonly ICustomLogger logger;

        private readonly Reader reader;

        public ReaderHooks(ICustomLogger logger, Reader reader)
        {
            this.logger = logger;
            this.reader = reader;

            On.RoR2.CostTypeCatalog.LunarItemOrEquipmentCostTypeHelper.PayCost += this.LunarItemOrEquipmentCostTypeHelper_PayCost;
            typeof(IL.RoR2.CostTypeCatalog).GetEvent("<Init>g__PayCostItems|5_1").AddHook(this, nameof(CostTypeCatalog_PayCostItems));
            IL.RoR2.LunarSunBehavior.FixedUpdate += this.LunarSunBehavior_FixedUpdate;
            IL.RoR2.CharacterMaster.TryCloverVoidUpgrades += this.CharacterMaster_TryCloverVoidUpgrades;

            On.RoR2.BasicPickupDropTable.Add += this.BasicPickupDropTable_Add;
            On.RoR2.ArenaMonsterItemDropTable.Add += this.ArenaMonsterItemDropTable_Add;
            On.RoR2.FreeChestDropTable.Add += this.FreeChestDropTable_Add;
            IL.RoR2.BossGroup.DropRewards += this.BossGroup_DropRewards;

            //On.RoR2.UI.PickupPickerPanel.Awake += this.PickupPickerPanel_Awake;
            On.RoR2.PickupPickerController.OnDisplayBegin += this.PickupPickerController_OnDisplayBegin;
        }

        private void PickupPickerController_OnDisplayBegin(On.RoR2.PickupPickerController.orig_OnDisplayBegin orig, PickupPickerController self, NetworkUIPromptController networkUIPromptController, LocalUser localUser, CameraRigController cameraRigController)
        {
            orig(self, networkUIPromptController, localUser, cameraRigController);
            // TODO: test if this works on remote client
            this.reader.SetPickupPanelInfo(self.panelInstanceController, this.reader.GetPickupPanelInfo(localUser.cachedMasterController, self.options.Select(x => x.pickupIndex)));
        }

        private void PickupPickerPanel_Awake(On.RoR2.UI.PickupPickerPanel.orig_Awake orig, RoR2.UI.PickupPickerPanel self)
        {
            orig(self);
            var message = new SyncPickupPickerPanelInfoMessage(this.reader.GetLocalUser().cachedMasterController.netId, self.pickerController.netId);
            message.Send(NetworkDestination.Server);
        }

        private void BasicPickupDropTable_Add(On.RoR2.BasicPickupDropTable.orig_Add orig, BasicPickupDropTable self, List<PickupIndex> sourceDropList, float chance)
        {
            this.logger.LogMethodCall();
            this.reader.AddToPickupDropTable(self, sourceDropList, chance);
        }

        private void ArenaMonsterItemDropTable_Add(On.RoR2.ArenaMonsterItemDropTable.orig_Add orig, ArenaMonsterItemDropTable self, List<PickupIndex> sourceDropList, float chance)
        {
            this.logger.LogMethodCall();
            this.reader.AddToPickupDropTable(self, sourceDropList, chance);
        }

        private void FreeChestDropTable_Add(On.RoR2.FreeChestDropTable.orig_Add orig, FreeChestDropTable self, List<PickupIndex> sourceDropList, float listWeight)
        {
            this.logger.LogMethodCall();
            this.reader.AddToPickupDropTable(self, sourceDropList, listWeight);
        }

        private void LunarItemOrEquipmentCostTypeHelper_PayCost(On.RoR2.CostTypeCatalog.LunarItemOrEquipmentCostTypeHelper.orig_PayCost orig, CostTypeDef costTypeDef, CostTypeDef.PayCostContext context)
        {
            // Completely replaced this method, as it's bugged for a cost of greater than one anyway

            this.logger.LogMethodCall();
            this.reader.PayLunarItemOrEquipment(context);
        }
    }
}
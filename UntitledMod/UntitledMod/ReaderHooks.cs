using RoR2;
using System.Collections.Generic;

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

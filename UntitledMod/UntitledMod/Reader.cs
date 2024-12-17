using RoR2;
using RoR2.Items;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UntitledMod
{
    using static RoR2.CostTypeCatalog.LunarItemOrEquipmentCostTypeHelper;

    public partial class Reader
    {
        private readonly CustomLogger logger;

        private readonly InventoriesInfo inventoriesInfo;

        public Reader(CustomLogger logger, InventoriesInfo inventoriesInfo)
        {
            this.logger = logger;
            this.inventoriesInfo = inventoriesInfo;

            On.RoR2.CostTypeCatalog.LunarItemOrEquipmentCostTypeHelper.PayCost += this.LunarItemOrEquipmentCostTypeHelper_PayCost;
            typeof(IL.RoR2.CostTypeCatalog).GetEvent("<Init>g__PayCostItems|5_1").AddHook(this, nameof(CostTypeCatalog_PayCostItems));
            IL.RoR2.LunarSunBehavior.FixedUpdate += this.LunarSunBehavior_FixedUpdate;
            IL.RoR2.CharacterMaster.TryCloverVoidUpgrades += this.CharacterMaster_TryCloverVoidUpgrades;
        }

        private void DeprioritiseItemsInList(List<ItemIndex> list, CharacterMaster master)
        {
            if (!this.inventoriesInfo.Lookup(master, out var inventoryManager))
            {
                return;
            }

            var deprioritisedItems = list.Where(inventoryManager.WantsToKeep).ToArray();

            foreach (var item in deprioritisedItems)
            {
                list.Remove(item);
            }

            list.AddRange(deprioritisedItems);
        }

        private void LunarItemOrEquipmentCostTypeHelper_PayCost(On.RoR2.CostTypeCatalog.LunarItemOrEquipmentCostTypeHelper.orig_PayCost orig, CostTypeDef costTypeDef, CostTypeDef.PayCostContext context)
        {
            // Completely replace this method, as it's bugged for a cost of greater than one anyway

            Inventory inventory = context.activator.GetComponent<CharacterBody>().inventory;
            int cost = context.cost;

            var list = new List<object>();

            for (int i = 0; i < lunarItemIndices.Length; i++)
            {
                ItemIndex itemIndex = lunarItemIndices[i];
                list.AddRange(Enumerable.Repeat(itemIndex, inventory.GetItemCount(itemIndex)).Cast<object>());
            }

            int equipmentSlotCount = inventory.GetEquipmentSlotCount();

            for (uint j = 0; j < equipmentSlotCount; j++)
            {
                if (lunarEquipmentIndices.Contains(inventory.GetEquipment(j).equipmentIndex))
                {
                    list.Add(j);
                }
            }

            Util.ShuffleList(list, context.rng);

            if (this.inventoriesInfo.Lookup(context.activatorMaster, out var inventoryManager))
            {
                var deprioritisedItems = list.OfType<ItemIndex>().Where(inventoryManager.WantsToKeep).ToArray();

                foreach (var item in deprioritisedItems)
                {
                    list.Remove(item);
                }

                list.AddRange(deprioritisedItems.Cast<object>());
            }

            for (int k = 0; k < cost; k++)
            {
                var itemOrSlot = list[k];

                switch (itemOrSlot)
                {
                    case ItemIndex itemIndex:
                        inventory.RemoveItem(itemIndex);
                        context.results.itemsTaken.Add(itemIndex);
                        break;

                    case uint slot:
                        var equipmentIndex = inventory.GetEquipment(slot).equipmentIndex;
                        inventory.SetEquipment(EquipmentState.empty, slot);
                        context.results.equipmentTaken.Add(equipmentIndex);
                        break;
                }
            }

            MultiShopCardUtils.OnNonMoneyPurchase(context);
        }
    }
}
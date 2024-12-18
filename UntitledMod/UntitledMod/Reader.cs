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
        private readonly ICustomLogger logger;

        private readonly InventoriesInfo inventoriesInfo;

        public Reader(ICustomLogger logger, InventoriesInfo inventoriesInfo)
        {
            this.logger = logger;
            this.inventoriesInfo = inventoriesInfo;

            On.RoR2.CostTypeCatalog.LunarItemOrEquipmentCostTypeHelper.PayCost += this.LunarItemOrEquipmentCostTypeHelper_PayCost;
            typeof(IL.RoR2.CostTypeCatalog).GetEvent("<Init>g__PayCostItems|5_1").AddHook(this, nameof(CostTypeCatalog_PayCostItems));
            IL.RoR2.LunarSunBehavior.FixedUpdate += this.LunarSunBehavior_FixedUpdate;
            IL.RoR2.CharacterMaster.TryCloverVoidUpgrades += this.CharacterMaster_TryCloverVoidUpgrades;

            On.RoR2.BasicPickupDropTable.Add += this.BasicPickupDropTable_Add;
            On.RoR2.ArenaMonsterItemDropTable.Add += this.ArenaMonsterItemDropTable_Add;
            On.RoR2.FreeChestDropTable.Add += this.FreeChestDropTable_Add;
            IL.RoR2.BossGroup.DropRewards += this.BossGroup_DropRewards;
        }

        private PickupIndex SelectReward(Xoroshiro128Plus rng, List<PickupIndex> list)
        {
            var weightedSelection = new WeightedSelection<PickupIndex>();

            foreach (var pickupIndex in list)
            {
                var weight = this.inventoriesInfo.GetPickupWeightMultiplier(pickupIndex);

                if (weight > 0)
                {
                    weightedSelection.AddChoice(pickupIndex, weight);
                }
            }

            if (weightedSelection.Count > 0)
            {
                return weightedSelection.Evaluate(rng.nextNormalizedFloat);
            }

            return PickupIndex.none;
        }

        private PickupIndex SelectReward(PickupIndex bossItem, PickupIndex normalItem, Xoroshiro128Plus rng)
        {
            return rng.nextNormalizedFloat < this.inventoriesInfo.GetPickupWeightMultiplier(bossItem) ? bossItem : normalItem;
        }

        private void BasicPickupDropTable_Add(On.RoR2.BasicPickupDropTable.orig_Add orig, BasicPickupDropTable self, List<PickupIndex> sourceDropList, float chance)
        {
            if (chance <= 0f || sourceDropList.Count == 0)
            {
                return;
            }

            foreach (PickupIndex sourceDrop in sourceDropList)
            {
                var modifiedChance = chance * this.inventoriesInfo.GetPickupWeightMultiplier(sourceDrop);

                if (modifiedChance > 0 && (!self.IsFilterRequired() || self.PassesFilter(sourceDrop)))
                {
                    self.selector.AddChoice(sourceDrop, modifiedChance);
                }
            }
        }

        private void ArenaMonsterItemDropTable_Add(On.RoR2.ArenaMonsterItemDropTable.orig_Add orig, ArenaMonsterItemDropTable self, List<PickupIndex> sourceDropList, float chance)
        {
            if (chance <= 0f || sourceDropList.Count == 0)
            {
                return;
            }

            foreach (PickupIndex sourceDrop in sourceDropList)
            {
                var modifiedChance = chance * this.inventoriesInfo.GetPickupWeightMultiplier(sourceDrop);

                if (modifiedChance > 0 && self.PassesFilter(sourceDrop))
                {
                    self.selector.AddChoice(sourceDrop, modifiedChance);
                }
            }
        }

        private void FreeChestDropTable_Add(On.RoR2.FreeChestDropTable.orig_Add orig, FreeChestDropTable self, List<PickupIndex> sourceDropList, float listWeight)
        {
            if (listWeight <= 0 || sourceDropList.Count == 0)
            {
                return;
            }

            float weight = listWeight / sourceDropList.Count;

            foreach (PickupIndex value in sourceDropList)
            {
                var modifiedWeight = this.inventoriesInfo.GetPickupWeightMultiplier(value) * weight;

                if (modifiedWeight > 0)
                {
                    self.selector.AddChoice(value, modifiedWeight);
                }
            }
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
using RoR2;
using RoR2.Items;
using RoR2.UI;
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

        private readonly Func<ItemIndex, PickupIndex> findPickupIndex;

        public Reader(ICustomLogger logger, InventoriesInfo inventoriesInfo, Func<ItemIndex, PickupIndex> findPickupIndex)
        {
            this.logger = logger;
            this.inventoriesInfo = inventoriesInfo;
            this.findPickupIndex = findPickupIndex;
        }

        public bool PlayerWantsToKeep(CharacterMaster characterMaster, ItemIndex itemIndex)
        {
            return this.inventoriesInfo.Lookup(characterMaster, out var inventoryManager) && inventoryManager.WantsToKeep(itemIndex);
        }

        public PickupIndex SelectRewardFromList(Xoroshiro128Plus rng, List<PickupIndex> list)
        {
            this.logger.LogMethodCall();
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

        public PickupIndex SelectReward(PickupIndex bossItem, PickupIndex normalItem, Xoroshiro128Plus rng)
        {
            return rng.nextNormalizedFloat < this.inventoriesInfo.GetPickupWeightMultiplier(bossItem) ? bossItem : normalItem;
        }

        public void DeprioritiseItemsInList(List<ItemIndex> list, CharacterMaster master)
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

        public void AddToPickupDropTable(BasicPickupDropTable self, List<PickupIndex> sourceDropList, float chance)
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

        public void AddToPickupDropTable(ArenaMonsterItemDropTable self, List<PickupIndex> sourceDropList, float chance)
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

        public void AddToPickupDropTable(FreeChestDropTable self, List<PickupIndex> sourceDropList, float listWeight)
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

        public void PayLunarItemOrEquipment(CostTypeDef.PayCostContext context)
        {
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

        public float GetPickupWeightMultiplier(PickupIndex index)
        {
            return this.inventoriesInfo.GetPickupWeightMultiplier(index);
        }

        public LocalUser GetLocalUser()
        {
            return LocalUserManager.readOnlyLocalUsersList.SingleOrDefault();
        }

        internal bool[] GetPickupPanelInfo(PlayerCharacterMasterController playerCharacterMasterController, IEnumerable<PickupIndex> enumerable)
        {
            if (!this.inventoriesInfo.Lookup(playerCharacterMasterController.master, out var inventoryManager))
            {
                throw new InvalidOperationException();
            }

            var bannedPickups = inventoryManager.GetBannedItems().Select(this.findPickupIndex).ToArray();
            return enumerable.Select(x => !bannedPickups.Contains(x)).ToArray();
        }

        internal void SetPickupPanelInfo(PickupPickerPanel panel, bool[] info)
        {
            for (int i = 0; i < info.Length; i++)
            {
                if (!info[i])
                {
                    panel.RemovePickupButtonAvailability(i);
                }
            }
        }
    }
}
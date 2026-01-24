using RoR2;
using RoR2.Items;
using System.Collections.Generic;
using System.Linq;

namespace AssortedExperiments.Items
{
    public class MinionInheritOnKillBodyBehavior : BaseItemBodyBehavior
    {
        [ItemDefAssociation(useOnServer = true, useOnClient = false)]
        public static ItemDef GetItemDef()
        {
            return Content.Items.MinionInheritOnKill;
        }

        public static IEnumerable<CharacterMaster> GetMinions(CharacterMaster? master)
        {
            var minionGroup = master.Then(x => MinionOwnership.MinionGroup.FindGroup(x!.netId));

            if (minionGroup != null)
            {
                foreach (var minion in minionGroup.members)
                {
                    var minionMaster = minion.Then(x => x.GetComponent<CharacterMaster>());
                    if (minionMaster && minionMaster!.hasBody)
                    {
                        yield return minionMaster!;
                    }
                }
            }
        }

        public void UpdateInventory(Inventory minionInventory, int newStack)
        {
            var alreadyOwnedInheritableItems = GetItems(minionInventory.permanentItemStacks).Where(IsInheritable).ToArray();
            var inheritableItems = GetItems(this.body.inventory.permanentItemStacks).Where(IsInheritable).Except(alreadyOwnedInheritableItems).ToArray();

            int numberOfInheritableItemsWanted = newStack * 2 - alreadyOwnedInheritableItems.Length;

            if (numberOfInheritableItemsWanted > 0)
            {
                int numberOfItemsAdded = 0;

                foreach (var item in inheritableItems.OrderBy(x => x.tier, ItemTierComparer.Instance))
                {
                    if (numberOfItemsAdded >= numberOfInheritableItemsWanted)
                    {
                        break;
                    }

                    if (minionInventory.GetItemCountPermanent(item) > 0)
                    {
                        continue;
                    }

                    // TODO: for devotion lemurians, this is overidden by the call to CleanInventory in DevotionInventoryController.UpdateMinionInventory.
                    minionInventory.GiveItemPermanent(item, 1);
                    numberOfItemsAdded++;
                }
            }

            static bool IsInheritable(ItemDef itemDef)
            {
                return itemDef.ContainsTag(ItemTag.OnKillEffect) && !itemDef.ContainsTag(ItemTag.CannotCopy);
            }
        }

        private static IEnumerable<ItemDef> GetItems(ItemCollection itemCollection)
        {
            List<ItemIndex> output = new List<ItemIndex>();
            itemCollection.GetNonZeroIndices(output);
            return output.Select(ItemCatalog.GetItemDef);
        }

        private void OnEnable()
        {
            this.UpdateAllMinions(this.stack);
            MasterSummon.onServerMasterSummonGlobal += this.OnServerMasterSummonGlobal;
            this.body.onInventoryChanged += this.OnInventoryChanged;
        }

        private void OnDisable()
        {
            this.body.onInventoryChanged -= this.OnInventoryChanged;
            MasterSummon.onServerMasterSummonGlobal -= this.OnServerMasterSummonGlobal;
            this.UpdateAllMinions(0);
        }

        private void OnInventoryChanged()
        {
            this.UpdateAllMinions(this.stack);
        }

        private void OnServerMasterSummonGlobal(MasterSummon.MasterSummonReport summonReport)
        {
            var master = this.body.Then(x => x.master);

            if (master && master == summonReport.leaderMasterInstance)
            {
                CharacterMaster summonMasterInstance = summonReport.summonMasterInstance;
                if (summonMasterInstance)
                {
                    CharacterBody body = summonMasterInstance.GetBody();
                    if (body)
                    {
                        this.UpdateInventory(summonMasterInstance.inventory, this.stack);
                    }
                }
            }
        }

        private void UpdateAllMinions(int newStack)
        {
            if (this.body)
            {
                foreach (var minionMaster in GetMinions(this.body.Then(x => x.master)))
                {
                    var minionInventory = minionMaster!.inventory;

                    if (minionInventory)
                    {
                        this.UpdateInventory(minionMaster.inventory, newStack);
                    }
                }
            }
        }

        private sealed class ItemTierComparer : IComparer<ItemTier>
        {
            private ItemTierComparer()
            {
            }

            public static ItemTierComparer Instance { get; } = new ItemTierComparer();

            public int Compare(ItemTier x, ItemTier y)
            {
                return GetTierValue(x).CompareTo(GetTierValue(y));
            }

            private static sbyte GetTierValue(ItemTier tier)
            {
                switch (tier)
                {
                    case ItemTier.Tier1:
                    case ItemTier.VoidTier1:
                        return 0;

                    case ItemTier.Tier2:
                    case ItemTier.VoidTier2:
                        return 1;

                    default:
                        return 2;
                }
            }
        }
    }
}
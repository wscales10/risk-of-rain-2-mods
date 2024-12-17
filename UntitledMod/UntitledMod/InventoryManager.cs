using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UntitledMod
{
    public interface IReadOnlyInventoryManager
    {
        bool WantsToKeep(ItemIndex itemIndex);
    }

    public interface IInventoryManager : IReadOnlyInventoryManager
    {
        void OnPickupItem(ItemIndex itemIndex);

        void OnLoseItem(ItemIndex itemIndex);
    }

    public class InventoryManager : IInventoryManager
    {
        private static ItemIndex[] visibleDamageItems;

        private readonly HashSet<ItemIndex> allowedVisibleDamageItems = new HashSet<ItemIndex>();

        private readonly CustomLogger logger;

        private bool isSublistLocked = false;

        public InventoryManager(CustomLogger logger)
        {
            this.logger = logger;
        }

        public static void Init()
        {
            ItemDef[] invisibleDamageItems = new[] // TODO: consider specifying this elsewhere and including non-damage items
            {
                RoR2Content.Items.BossDamageBonus,
                RoR2Content.Items.WarCryOnMultiKill,
                DLC2Content.Items.LowerHealthHigherDamage,
                DLC2Content.Items.IncreaseDamageOnMultiKill,
                RoR2Content.Items.Crowbar,
                RoR2Content.Items.DeathMark,
                DLC1Content.Items.FragileDamageBonus,
                RoR2Content.Items.NearbyDamageBonus,
                DLC1Content.Items.StrengthenBurn,
                RoR2Content.Items.ShinyPearl,
                DLC1Content.Items.CritDamage,
                RoR2Content.Items.CritGlasses,
                DLC2Content.Items.OnLevelUpFreeUnlock,
                DLC1Content.Items.CritGlassesVoid,
                DLC1Content.Items.AttackSpeedAndMoveSpeed,
                RoR2Content.Items.ExecuteLowHealthElite,
                DLC1Content.Items.MoreMissile,
                RoR2Content.Items.AttackSpeedOnCrit,
                RoR2Content.Items.LunarDagger,
                RoR2Content.Items.ArmorReductionOnHit,
                RoR2Content.Items.BoostAttackSpeed,
                DLC1Content.Items.PermanentDebuffOnHit,
                RoR2Content.Items.EnergizedOnEquipmentUse,
            }.Where(d => !(d is null)).ToArray();

            visibleDamageItems = ItemCatalog.GetItemsWithTag(ItemTag.Damage).Except(invisibleDamageItems.Select(d => d.itemIndex)).ToArray();
        }

        public bool WantsToKeep(ItemIndex itemIndex)
        {
            this.logger.LogMethodCall();
            return this.isSublistLocked && this.allowedVisibleDamageItems.Contains(itemIndex);
        }

        public void OnPickupItem(ItemIndex itemIndex)
        {
            this.logger.LogDebug($"Picking up '{ItemCatalog.GetItemDef(itemIndex).name}'");
            if (this.isSublistLocked)
            {
                return;
            }

            if (visibleDamageItems.Contains(itemIndex))
            {
                this.allowedVisibleDamageItems.Add(itemIndex);

                if (this.allowedVisibleDamageItems.Count > 4) // TODO: this number should be specified elsewhere
                {
                    this.isSublistLocked = true;

                    foreach (var i in this.allowedVisibleDamageItems)
                    {
                        Chat.SendBroadcastChat(new ColoredTokenChatMessage { baseToken = ItemCatalog.GetItemDef(i).nameToken });
                    }
                }
            }
        }

        public void OnLoseItem(ItemIndex itemIndex)
        {
            this.logger.LogDebug($"Lost '{ItemCatalog.GetItemDef(itemIndex).name}'");
            
            if (!this.isSublistLocked)
            {
                this.allowedVisibleDamageItems.Remove(itemIndex);
            }
        }
    }
}
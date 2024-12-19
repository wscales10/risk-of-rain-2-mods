using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;

namespace UntitledMod
{
    public class InventoryManager : IInventoryManager
    {
        private const int maxVisibleDamageItems = 5; // TODO: this number should be specified elsewhere

        private static ItemIndex[] visibleDamageItems;

        private readonly HashSet<ItemIndex> allowedVisibleDamageItems = new HashSet<ItemIndex>();

        private readonly ICustomLogger logger;

        private bool isSublistLocked = false;

        public InventoryManager(ICustomLogger logger)
        {
            this.logger = logger;
        }

        public event NotifyCollectionChangedEventHandler BannedItemsChanged;

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

        public bool IsAllowed(ItemIndex itemIndex)
        {
            return !this.GetBannedItems().Contains(itemIndex);
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
                if (this.allowedVisibleDamageItems.Add(itemIndex))
                {
                    Chat.SendBroadcastChat(new ColoredTokenChatMessage
                    {
                        baseToken = "[{1} {2}]",
                        paramTokens = new[] { "Acquired", ItemCatalog.GetItemDef(itemIndex).nameToken },
                        paramColors = new[] { new Color32(255, 255, 255, 255), itemIndex.GetItemColor() }
                    });
                    Chat.SendBroadcastChat(new ColoredTokenChatMessage { baseToken = $"[{this.allowedVisibleDamageItems.Count}/{maxVisibleDamageItems} slots filled]" });
                }

                if (this.allowedVisibleDamageItems.Count >= maxVisibleDamageItems)
                {
                    this.isSublistLocked = true;
                    this.BannedItemsChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, this.GetBannedItems().ToArray()));
                }
            }
        }

        public void OnLoseItem(ItemIndex itemIndex)
        {
            this.logger.LogDebug($"Lost '{ItemCatalog.GetItemDef(itemIndex).name}'");

            if (!this.isSublistLocked)
            {
                if (this.allowedVisibleDamageItems.Remove(itemIndex))
                {
                    Chat.SendBroadcastChat(new ColoredTokenChatMessage
                    {
                        baseToken = "[{1} {2}]",
                        paramTokens = new[] { "Lost", ItemCatalog.GetItemDef(itemIndex).nameToken },
                        paramColors = new[] { new Color32(255, 255, 255, 255), itemIndex.GetItemColor() }
                    });
                    Chat.SendBroadcastChat(new ColoredTokenChatMessage { baseToken = $"[{this.allowedVisibleDamageItems.Count}/{maxVisibleDamageItems} slots filled]" });
                }
            }
        }

        private IEnumerable<ItemIndex> GetBannedItems()
        {
            if (!this.isSublistLocked)
            {
                return Enumerable.Empty<ItemIndex>();
            }

            return visibleDamageItems.Except(this.allowedVisibleDamageItems);
        }
    }
}
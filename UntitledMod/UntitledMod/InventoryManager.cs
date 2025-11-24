using Newtonsoft.Json.Utilities;
using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UntitledMod.Context;

namespace UntitledMod
{
    public class InventoryManager : NetworkBehaviour, IInventoryManager
    {
        private const int maxVisibleDamageItems = 5; // TODO: this number should be specified elsewhere

        private readonly HashSet<ItemIndex> allowedVisibleDamageItems = new HashSet<ItemIndex>();

        private readonly HashSet<ItemIndex> bannedItems = new HashSet<ItemIndex>();

        private ICustomLogger logger;

        private IRoR2Context gameContext;

        private bool isSublistLocked = false;

        private VisibleDamageItemsProvider controlledItemsProvider;

        public event NotifyCollectionChangedEventHandler BannedItemsChanged;

        public InventoryManager Init(ICustomLogger logger, IRoR2Context context, VisibleDamageItemsProvider visibleDamageItemsProvider)
        {
            this.logger = logger;
            this.gameContext = context;
            this.controlledItemsProvider = visibleDamageItemsProvider;
            return this;
        }

        public void Reset()
        {
            if (!this.gameContext.IsNetworkServerActive)
            {
                return;
            }

            this.logger.LogMethodCall();
            this.allowedVisibleDamageItems.Clear();
            this.SetIsSublistLocked(false);
        }

        public bool WantsToKeep(ItemIndex itemIndex)
        {
            this.logger.LogMethodCall();
            return this.isSublistLocked && this.allowedVisibleDamageItems.Contains(itemIndex);
        }

        public bool IsAllowed(ItemIndex itemIndex)
        {
            return !this.bannedItems.Contains(itemIndex);
        }

        public void OnPickupItem(ItemIndex itemIndex)
        {
            this.gameContext.ThrowIfClient();
            this.logger.LogDebug($"Picking up '{ItemCatalog.GetItemDef(itemIndex).name}'");
            if (this.isSublistLocked)
            {
                return;
            }

            if (this.controlledItemsProvider.GetItems().Contains(itemIndex))
            {
                bool wasAdded = this.allowedVisibleDamageItems.Add(itemIndex);

                if (this.allowedVisibleDamageItems.Count >= maxVisibleDamageItems)
                {
                    this.SetIsSublistLocked(true);
                }

                if (wasAdded)
                {
                    Chat.SendBroadcastChat(new ColoredTokenChatMessage
                    {
                        baseToken = "[{1} {2}]",
                        paramTokens = new[] { "Acquired", ItemCatalog.GetItemDef(itemIndex).nameToken },
                        paramColors = new[] { new Color32(255, 255, 255, 255), itemIndex.GetItemColor() }
                    });
                    Chat.SendBroadcastChat(new ColoredTokenChatMessage { baseToken = $"[{this.allowedVisibleDamageItems.Count}/{maxVisibleDamageItems} slots filled]" });
                }
            }
        }

        public void OnLoseItem(ItemIndex itemIndex)
        {
            this.gameContext.ThrowIfClient();
            this.logger.LogDebug($"Lost '{ItemCatalog.GetItemDef(itemIndex).name}'");

            if (!this.isSublistLocked)
            {
                if (this.allowedVisibleDamageItems.Remove(itemIndex))
                {
                    this.TargetUpdateClient(this.connectionToClient, this.allowedVisibleDamageItems.ToArray());
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

        public IEnumerable<ItemIndex> GetBannedItems()
        {
            foreach (var item in this.bannedItems)
            {
                yield return item;
            }
        }

        private void SetIsSublistLocked(bool value)
        {
            if (this.isSublistLocked != value)
            {
                this.isSublistLocked = value;

                if (this.isSublistLocked)
                {
                    if (this.bannedItems.Count != 0)
                    {
                        throw new InvalidOperationException("Expected banned item count to be 0");
                    }

                    foreach (var item in this.controlledItemsProvider.GetItems().Except(this.allowedVisibleDamageItems))
                    {
                        this.bannedItems.Add(item);
                    }

                    this.BannedItemsChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, this.bannedItems.ToArray()));
                }
                else
                {
                    var unbannedItems = this.bannedItems.ToArray();
                    this.bannedItems.Clear();
                    this.BannedItemsChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, unbannedItems));
                }
            }

            if (this.connectionToClient != null)
            {
                this.TargetUpdateClient(this.connectionToClient, this.allowedVisibleDamageItems.ToArray(), this.isSublistLocked = value);
            }
        }

        [TargetRpc]
        private void TargetUpdateClient(NetworkConnection _, ItemIndex[] allowedVisibleDamageItems, bool? isSublistLocked = null)
        {
            this.gameContext.ThrowIfServer();

            this.allowedVisibleDamageItems.Clear();

            foreach (var item in allowedVisibleDamageItems)
            {
                this.allowedVisibleDamageItems.Add(item);
            }

            if (isSublistLocked.HasValue)
            {
                this.SetIsSublistLocked(isSublistLocked.Value);
            }
        }
    }
}
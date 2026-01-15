using BepInEx.Logging;
using RoR2;
using RoR2.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace AssortedExperiments
{
    internal class Hooks
    {
        protected readonly ManualLogSource logger;

        protected readonly Settings settings;

        protected readonly HashSet<SceneDirector> waitingForScrapper;

        protected Hooks(ManualLogSource logger, Settings settings, HashSet<SceneDirector> waitingForScrapper)
        {
            this.logger = logger;
            this.settings = settings;
            this.waitingForScrapper = waitingForScrapper;
        }

        protected static float GetUnownedItemProbability(int ownedCount, int poolSize, float adjustmentStrength = 1)
        {
            if (poolSize == 0)
            {
                return 0;
            }

            var ownedProportion = ownedCount / (float)poolSize;
            var output = Mathf.Pow(19.1f, -Mathf.Sqrt(ownedProportion));
            return adjustmentStrength * output + (1 - adjustmentStrength) * (1 - ownedProportion);
        }

        protected static bool IsScrapper(DirectorCard? directorCard)
        {
            return directorCard?.spawnCard?.name?.Contains("Scrapper") == true;
        }

        // TODO it's a bit janky to have separate methods like this. What if I want for shop
        // terminal for player? What if I add another variable? The number of methods could
        // exponentionally increase.
        protected Func<PickupDropTable, ShopTerminalBehavior, PickupDropTable> RandomlyTransformDropTableForShopTerminal([CallerMemberName] string? context = null) => (table, shopTerminal) =>
        {
            return this.RandomlyTransformDropTableInternal(table, null, shopTerminal, context);
        };

        protected Func<PickupDropTable, PickupDropTable> RandomlyTransformDropTable([CallerMemberName] string? context = null) => (table) =>
        {
            return this.RandomlyTransformDropTableInternal(table, null, null, context);
        };

        protected bool ShouldTryRollToSeeIfShouldStack()
        {
            // TODO: ensure this still activates normally if a player decides to spend ages on stage 1 with artifact of sacrifice.
            // Mayyybe control that with a config setting, as it might be fun to try to stay on
            // stage 1 a bit longer to try to get all the items you want?
            return this.settings.TestMode || Run.instance?.stageClearCount > 0;
        }

        protected Func<PickupDef, bool> GetFilter(PlayerCharacterMasterController? player)
        {
            if (this.settings.TestMode)
            {
                return this.TestFilter;
            }

            if (player)
            {
                return this.IsOwnedByPlayer(player!.master);
            }

            return this.IsOwnedByAnyLivingPlayer;
        }

        protected Func<PickupDropTable, PlayerCharacterMasterController, PickupDropTable> RandomlyTransformDropTableForPlayer([CallerMemberName] string? context = null) => (table, player) =>
        {
            return this.RandomlyTransformDropTableInternal(table, player, null, context);
        };

        private static bool HasEquipment(Inventory inventory, EquipmentIndex equipmentIndex)
        {
            return inventory._equipmentStateSlots.SelectMany(x => x).Any(v => v.equipmentIndex == equipmentIndex);
        }

        /// <remarks>Designed to be used immediately before rolling and never again.</remarks>
        private static PickupDropTable SoftFilter(PickupDropTable originalTable, Func<UniquePickup, bool> filter, UnownedItemProbabilityGetter getUnownedItemProbability, float adjustmentStrength)
        {
            WeightedSelection<UniquePickup> originalSelector = PickupDropTableUtils.GetWeightedSelection(originalTable);
            var output = ScriptableObject.CreateInstance<TemporaryPickupDropTable>();
            output.Selector = PickupDropTableUtils.IncreaseWeights(originalSelector, filter, getUnownedItemProbability, adjustmentStrength);
            return output;
        }

        private bool WantItem(Inventory inventory, ItemIndex itemIndex)
        {
            var count = inventory.GetItemCountEffective(itemIndex);

            if (count < 1)
            {
                return false;
            }

            var currentPreferredMaxStacks = this.settings.PreferredMaxStacks;
            return currentPreferredMaxStacks < 1 || count < currentPreferredMaxStacks;
        }

        private bool IsOwnedByAnyLivingPlayer(PickupDef pickupDef)
        {
            var predicate = this.IsOwnedByPlayer(pickupDef);
            return PlayerCharacterMasterController.instances.Any(pcmc => predicate(pcmc.master)); // TODO: consider whether we want to filter for alive players. Probably not, right?
        }

        private Func<CharacterMaster, bool> IsOwnedByPlayer(PickupDef pickupDef)
        {
            if (pickupDef.droneIndex != DroneIndex.None)
            {
                return master => CharacterMaster.readOnlyInstancesList.Any(x => x.minionOwnership?.ownerMaster == master && DroneCatalog.FindDroneDefFromBody(x.originalBodyPrefab)?.droneIndex == pickupDef.droneIndex);
            }
            else if (pickupDef.itemIndex != ItemIndex.None)
            {
                return master =>
                {
                    var inventory = master.inventory;
                    if (this.WantItem(inventory, pickupDef.itemIndex))
                    {
                        return true;
                    }

                    var transformedItem = ContagiousItemManager.GetTransformedItemIndex(pickupDef.itemIndex);
                    if (transformedItem != ItemIndex.None && this.WantItem(inventory, transformedItem))
                    {
                        return true;
                    }

                    return false;
                };
            }
            else if (pickupDef.equipmentIndex != EquipmentIndex.None)
            {
                return master => HasEquipment(master.inventory, pickupDef.equipmentIndex);
            }
            else
            {
                this.logger.LogWarning($"Unknown pickup type for pickupDef {Language.GetString(pickupDef.nameToken)}.");
                return _ => false;
            }
        }

        private Func<PickupDef, bool> IsOwnedByPlayer(CharacterMaster playerCharacterMaster)
        {
            return pickupDef => this.IsOwnedByPlayer(pickupDef)(playerCharacterMaster);
        }

        private bool TestFilter(PickupDef pickupDef)
        {
            if (pickupDef.droneIndex != DroneIndex.None)
            {
                // TODO: put something meaningful here when you test alloyed collective
                return false;
            }
            else if (pickupDef.itemIndex != ItemIndex.None)
            {
                return ItemCatalog.GetItemDef(pickupDef.itemIndex).ContainsTag(ItemTag.Damage);
            }
            else if (pickupDef.equipmentIndex != EquipmentIndex.None)
            {
                var equipmentDef = EquipmentCatalog.GetEquipmentDef(pickupDef.equipmentIndex);
                return equipmentDef.cooldown < 25 || equipmentDef.isLunar && equipmentDef.cooldown < 46;
            }
            else
            {
                this.logger.LogWarning($"Unknown pickup type for pickupDef {Language.GetString(pickupDef.nameToken)}.");
                return false;
            }
        }

        private PickupDropTable RandomlyTransformDropTableInternal(PickupDropTable table, PlayerCharacterMasterController? player, ShopTerminalBehavior? shopTerminal, string? context)
        {
            if (this.ShouldTryRollToSeeIfShouldStack())
            {
                string? shopTerminalName = shopTerminal?.name;
                bool is3dPrinter = shopTerminalName?.Contains("Duplicator") ?? false;

                if (shopTerminalName != null)
                {
                    this.logger.LogDebug($"'{shopTerminalName}' {(is3dPrinter ? "is" : "is not")} a 3D printer.");
                }

                this.logger.LogDebug($"Applying soft filter to drop table in context '{context}' ({(is3dPrinter ? "is" : "not")} a 3D printer).");
                return SoftFilter(table, pickup => this.GetFilter(player)(PickupCatalog.GetPickupDef(pickup.pickupIndex)), GetUnownedItemProbability, is3dPrinter ? this.settings.AdjustmentStrengthFor3dPrinters : 1);
            }

            return table;
        }
    }
}
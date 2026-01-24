using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace AssortedExperiments.ItemBias
{
    public partial class ItemBias : Module
    {
        private FilterFactory filterFactory;

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
            return this.Settings.TestMode || Run.instance?.stageClearCount > 0;
        }

        protected Func<PickupDropTable, PlayerCharacterMasterController, PickupDropTable> RandomlyTransformDropTableForPlayer([CallerMemberName] string? context = null) => (table, player) =>
        {
            return this.RandomlyTransformDropTableInternal(table, player, null, context);
        };

        /// <remarks>Designed to be used immediately before rolling and never again.</remarks>
        private static PickupDropTable SoftFilter(PickupDropTable originalTable, Func<UniquePickup, bool> filter, UnownedItemProbabilityGetter getUnownedItemProbability, float adjustmentStrength)
        {
            WeightedSelection<UniquePickup> originalSelector = PickupDropTableUtils.GetWeightedSelection(originalTable);
            var output = ScriptableObject.CreateInstance<TemporaryPickupDropTable>();
            output.Selector = PickupDropTableUtils.IncreaseWeights(originalSelector, filter, getUnownedItemProbability, adjustmentStrength);
            return output;
        }

        private PickupDropTable RandomlyTransformDropTableInternal(PickupDropTable table, PlayerCharacterMasterController? player, ShopTerminalBehavior? shopTerminal, string? context)
        {
            if (this.ShouldTryRollToSeeIfShouldStack())
            {
                string? shopTerminalName = shopTerminal?.name;
                bool is3dPrinter = shopTerminalName?.Contains("Duplicator") ?? false;

                if (shopTerminalName != null)
                {
                    this.Logger.LogDebug($"'{shopTerminalName}' {(is3dPrinter ? "is" : "is not")} a 3D printer.");
                }

                this.Logger.LogDebug($"Applying soft filter to drop table in context '{context}' ({(is3dPrinter ? "is" : "not")} a 3D printer).");
                return SoftFilter(table, pickup => this.filterFactory.GetFilter(player).Filter(PickupCatalog.GetPickupDef(pickup.pickupIndex)), GetUnownedItemProbability, is3dPrinter ? this.Settings.AdjustmentStrengthFor3dPrinters : 1);
            }

            return table;
        }

        private PickupIndex[] RandomlyTransformPickupIndexArray(PickupIndex[] array, CharacterBody owner)
        {
            _ = this.TryRandomlyTransformPickupIndexArray(ref array, owner);
            return array;
        }

        private bool TryRandomlyTransformPickupIndexArray(ref PickupIndex[] array, CharacterBody owner)
        {
            if (!owner)
            {
                this.Logger.LogWarning("Owner character body is null in RandomlyTransformPickupIndexArray - consider getting master component instead, maybe through interactor.");
                return false;
            }

            if (!this.ShouldTryRollToSeeIfShouldStack())
            {
                return false;
            }

            Dictionary<PickupIndex, (bool shouldStackRarity, bool isOwned)> dictionary = array.ToDictionary(x => x, pickupIndex =>
            {
                var pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
                return (PickupDropTableUtils.ShouldTryStackRarity(PickupDropTableUtils.GetRarity(pickupDef)), this.filterFactory.GetFilter(owner.master?.playerCharacterMasterController).Filter(pickupDef));
            });

            PickupIndex[] filtered;

            bool tryPickUnownedItem = Run.instance.treasureRng.nextNormalizedFloat < GetUnownedItemProbability(dictionary.Count(x => x.Value.isOwned), dictionary.Count);

            // TODO: DRY
            if (tryPickUnownedItem)
            {
                filtered = array.Where(pickupIndex =>
                {
                    var (shouldStackRarity, isOwned) = dictionary[pickupIndex];
                    return !shouldStackRarity || !isOwned;
                }).ToArray();
            }
            else
            {
                filtered = array.Where(pickupIndex =>
                {
                    var (shouldStackRarity, isOwned) = dictionary[pickupIndex];
                    return !shouldStackRarity || isOwned;
                }).ToArray();
            }

            if (filtered.Length > 0)
            {
                array = filtered;
                return true;
            }

            return false;
        }

        private Func<DamageReport, PlayerCharacterMasterController?> GetAttackingPlayerFromDamageReport([CallerMemberName] string? context = null) => damageReport =>
        {
            this.Logger.LogDebug($"Getting attacking player from damage report in context {context}");
            return Utils.GetAttackingPlayerFromDamageReportInternal(damageReport);
        };
    }
}
using BepInEx.Logging;
using RoR2;
using RoR2.Items;
using System;
using System.Linq;

namespace AssortedExperiments.ItemBias
{
    public interface IFilter
    {
        public bool Filter(PickupDef pickupDef);
    }

    public class TestFilter : IFilter
    {
        private readonly ManualLogSource logger;

        public TestFilter(ManualLogSource logger)
        {
            this.logger = logger;
        }

        public bool Filter(PickupDef pickupDef)
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
    }

    public abstract class PlayerFilterBase
    {
        private readonly ManualLogSource logger;

        private readonly Settings settings;

        protected PlayerFilterBase(ManualLogSource logger, Settings settings)
        {
            this.logger = logger;
            this.settings = settings;
        }

        protected Func<CharacterMaster, bool> IsOwnedByPlayer(PickupDef pickupDef)
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
                return master => Utils.HasEquipment(master.inventory, pickupDef.equipmentIndex);
            }
            else
            {
                this.logger.LogWarning($"Unknown pickup type for pickupDef {Language.GetString(pickupDef.nameToken)}.");
                return _ => false;
            }
        }

        private bool WantItem(Inventory inventory, ItemIndex itemIndex)
        {
            var count = inventory.GetItemCountEffective(itemIndex);

            if (count < 1)
            {
                return false;
            }

            var itemDef = ItemCatalog.GetItemDef(itemIndex);
            var isLunar = itemDef && itemDef.tier == ItemTier.Lunar;
            var currentPreferredMaxStacks = isLunar ? this.settings.PreferredMaxStacksLunar : this.settings.PreferredMaxStacks;
            return currentPreferredMaxStacks < 1 || count < currentPreferredMaxStacks;
        }
    }

    public class PlayerFilter : PlayerFilterBase, IFilter
    {
        private readonly CharacterMaster playerCharacterMaster;

        public PlayerFilter(CharacterMaster playerCharacterMaster, ManualLogSource logger, Settings settings) : base(logger, settings)
        {
            this.playerCharacterMaster = playerCharacterMaster;
        }

        public bool Filter(PickupDef pickupDef)
        {
            return this.IsOwnedByPlayer(pickupDef)(this.playerCharacterMaster);
        }
    }

    public class AllPlayersFilter : PlayerFilterBase, IFilter
    {
        public AllPlayersFilter(ManualLogSource logger, Settings settings) : base(logger, settings)
        {
        }

        public bool Filter(PickupDef pickupDef)
        {
            var predicate = this.IsOwnedByPlayer(pickupDef);
            return PlayerCharacterMasterController.instances.Any(pcmc => predicate(pcmc.master)); // TODO: consider whether we want to filter for alive players. Probably not, right?
        }
    }
}
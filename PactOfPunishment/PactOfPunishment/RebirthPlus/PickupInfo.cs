using RoR2;

namespace PactOfPunishment.RebirthPlus
{
    public abstract class PickupInfo
    {
        public abstract bool IsAvailable { get; }

        public static implicit operator PickupInfo((ItemDef itemDef, uint count) tuple)
        {
            return new ItemStack(tuple.itemDef, tuple.count);
        }

        public static implicit operator PickupInfo(ItemDef itemDef)
        {
            return new ItemStack(itemDef, 1);
        }

        public static implicit operator PickupInfo(EquipmentDef equipmentDef)
        {
            return new Equipment(equipmentDef);
        }

        public abstract string CountAgnosticString { get; }

        public abstract void GiveTo(Inventory inventory);

        public class ItemStack : PickupInfo
        {
            public ItemStack(ItemDef itemDef, uint count)
            {
                this.ItemDef = itemDef;
                this.Count = count;
            }

            public ItemDef ItemDef { get; }

            public uint Count { get; }

            public override bool IsAvailable => Run.instance.IsItemAvailable(this.ItemDef.itemIndex);

            public override string CountAgnosticString => Language.GetString(this.ItemDef.nameToken);

            public override string ToString() => $"{this.Count} {this.CountAgnosticString}";

            public override void GiveTo(Inventory inventory) => inventory.GiveItemPermanent(this.ItemDef, (int)this.Count);
        }

        public class Equipment : PickupInfo
        {
            public Equipment(EquipmentDef equipmentDef)
            {
                this.EquipmentDef = equipmentDef;
            }

            public EquipmentDef EquipmentDef { get; }

            public override bool IsAvailable => Run.instance.IsEquipmentAvailable(this.EquipmentDef.equipmentIndex);

            public override string CountAgnosticString => Language.GetString(this.EquipmentDef.nameToken);

            public override string ToString() => this.CountAgnosticString;

            public override void GiveTo(Inventory inventory) => inventory.SetEquipmentIndex(this.EquipmentDef.equipmentIndex, false);
        }
    }
}
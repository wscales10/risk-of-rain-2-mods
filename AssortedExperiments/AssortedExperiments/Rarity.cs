using RoR2;
using System.Collections.Generic;

namespace AssortedExperiments
{
    public class Rarity
    {
        public virtual ItemTier Tier => ItemTier.NoTier;
    }

    public class ItemRarity : Rarity
    {
        public ItemRarity(ItemTier tier)
        {
            this.Tier = tier;
        }

        public override ItemTier Tier { get; }
    }

    public class EquipmentRarity : Rarity
    {
        private readonly bool isLunar;

        public EquipmentRarity(bool isLunar)
        {
            this.isLunar = isLunar;
        }

        public override ItemTier Tier => this.isLunar ? ItemTier.Lunar : ItemTier.NoTier;
    }

    public class DroneRarity : Rarity
    {
        public DroneRarity(ItemTier tier)
        {
            this.Tier = tier;
        }

        public override ItemTier Tier { get; }
    }

    public class ItemTierRarity : Rarity
    {
        public ItemTierRarity(ItemTier tier)
        {
            this.Tier = tier;
        }

        public override ItemTier Tier { get; }
    }

    // TODO: Consider using PickupTransmtuationManager instead.
    public class RarityEqualityComparer : IEqualityComparer<Rarity>
    {
        private RarityEqualityComparer()
        {
        }

        public static RarityEqualityComparer Instance { get; } = new RarityEqualityComparer();

        public bool Equals(Rarity? x, Rarity? y)
        {
            return x?.GetType() == y?.GetType() && x?.Tier == y?.Tier;
        }

        public int GetHashCode(Rarity obj)
        {
            return obj.Tier.GetHashCode();
        }
    }
}
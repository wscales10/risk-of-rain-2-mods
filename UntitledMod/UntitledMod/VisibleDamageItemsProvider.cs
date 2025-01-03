using RoR2;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace UntitledMod
{
    public class VisibleDamageItemsProvider
    {
        private static ReadOnlyCollection<ItemIndex> visibleDamageItems;

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

            visibleDamageItems = new ReadOnlyCollection<ItemIndex>(ItemCatalog.GetItemsWithTag(ItemTag.Damage).Except(invisibleDamageItems.Select(d => d.itemIndex)).ToArray());
        }

        public IEnumerable<ItemIndex> GetItems() => visibleDamageItems;
    }
}
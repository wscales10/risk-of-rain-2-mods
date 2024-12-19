using RoR2;
using UnityEngine;

namespace UntitledMod
{
    internal static class RoR2Utils
    {
        public static Color32 GetItemColor(this ItemIndex itemIndex)
        {
            return ColorCatalog.GetColor(ItemTierCatalog.GetItemTierDef(ItemCatalog.GetItemDef(itemIndex).tier).colorIndex);
        }
    }
}

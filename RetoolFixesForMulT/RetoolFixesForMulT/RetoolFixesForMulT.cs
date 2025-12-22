using BepInEx;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;

namespace RetoolFixesForMulT
{
    [BepInPlugin("com.woodyscales.retoolfixesformult", "MUL-T Retool Fixes", "1.0.0")]
    public class RetoolFixesForMulT : BaseUnityPlugin
    {
        public void Awake()
        {
            IL.RoR2.Inventory.SetActiveEquipmentSlot += this.Inventory_SetActiveEquipmentSlot;
        }

        private void Inventory_SetActiveEquipmentSlot(ILContext il)
        {
            var c = new ILCursor(il);

            if (!c.TryGotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchCall<Inventory>($"get_{nameof(Inventory.activeEquipmentSlot)}"),
                x => x.MatchCall<Inventory>(nameof(Inventory.GetEquipmentSetCount))))
            {
                this.Logger.LogWarning("Unable to apply MUL-T Retool fix. This mod either needs removing or updating.");
                return;
            }

            c.RemoveRange(2);
            c.Emit(OpCodes.Ldarg_1);
        }
    }
}
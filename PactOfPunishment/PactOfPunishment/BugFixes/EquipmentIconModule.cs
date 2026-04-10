using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.UI;
using System;
using System.Diagnostics;
using UnityEngine;

namespace PactOfPunishment.BugFixes
{
    public class EquipmentIconModule : Module
    {
        public override void Init()
        {
            On.RoR2.UI.EquipmentIcon.Awake += this.EquipmentIcon_Awake;
			IL.RoR2.UI.EquipmentIcon.Awake += Utils.HookIL(EquipmentIcon_Awake);
        }

        private static void EquipmentIcon_Awake(ILCursor c)
        {
			c.GotoNext(x => x.MatchCallvirt<HUD>($"get_{nameof(HUD.localUserViewer)}"));
			c.Remove();
			c.EmitDelegate<Func<HUD, LocalUser?>>(hud => hud ? hud.localUserViewer : null);

			ILLabel? label = null;

			c.GotoNext(MoveType.After,
				x => x.MatchLdarg(0),
				x => x.MatchLdfld<EquipmentIcon>(nameof(EquipmentIcon.cooldownText)),
				x => x.MatchCall<UnityEngine.Object>("op_Implicit"),
				x => x.MatchBrfalse(out label));
			c.Emit(OpCodes.Ldarg_0);
			c.Emit(OpCodes.Ldfld, AccessTools.DeclaredField(typeof(EquipmentIcon), nameof(EquipmentIcon.cooldownText)));
			c.Emit(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Component), nameof(Component.gameObject)));
			c.EmitDelegate<Func<GameObject, bool>>(gameObject => gameObject);
			c.Emit(OpCodes.Brfalse_S, label);
        }

        private void EquipmentIcon_Awake(On.RoR2.UI.EquipmentIcon.orig_Awake orig, RoR2.UI.EquipmentIcon self)
        {
            try
            {
                orig(self);
            }
            catch (Exception ex)
            {
                this.Logger.LogDebug($"hud: '{self.GetComponentInParent<HUD>()}'");
                this.Logger.LogDebug($"localUserViewer: '{self.GetComponentInParent<HUD>()?.localUserViewer}'");
                this.Logger.LogDebug($"cooldownText gameObject: '{self.cooldownText?.gameObject}'");

                Debugger.Break();
                this.Logger.LogError(ex);
            }
        }
    }
}
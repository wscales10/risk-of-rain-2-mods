using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using UnityEngine;

namespace PactOfPunishment
{
    [RequireComponent(typeof(InfiniteTowerWaveController))]
    public class SafeZoneRadiusCapper : MonoBehaviour
    {
        public float MaximumRadiusPercentage = 1;
    }

    public class CapSafeZoneRadius : Module
    {
        public override void Init()
        {
            IL.EntityStates.InfiniteTowerSafeWard.Active.FixedUpdate += Utils.HookIL(Active_FixedUpdate);
        }

        private static void Active_FixedUpdate(ILCursor c)
        {
            c.GotoNext(x => x.MatchCallvirt<InfiniteTowerWaveController>($"get_{nameof(InfiniteTowerWaveController.zoneRadiusPercentage)}"));
            c.Emit(OpCodes.Dup);
            c.Index++;
            c.EmitDelegate<Func<InfiniteTowerWaveController, float, float>>((self, orig) =>
            {
                if (self.TryGetComponent<SafeZoneRadiusCapper>(out var behavior))
                {
                    orig = Mathf.Min(orig, behavior.MaximumRadiusPercentage);
                }

                return orig;
            });
        }
    }
}
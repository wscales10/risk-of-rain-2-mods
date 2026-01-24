using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using UnityEngine;

namespace AssortedExperiments.ItemAdjustments
{
    public class Transcendence : Module
    {
        public override void Init()
        {
            // Nerf Transcendence
            IL.RoR2.GlobalEventManager.OnCharacterHitGroundServer += GlobalEventManager_OnCharacterHitGroundServer;
        }

        private static void GlobalEventManager_OnCharacterHitGroundServer(ILContext il)
        {
            var c = new ILCursor(il);
            c.GotoNext(
                x => x.MatchLdloc(7),
                x => x.MatchLdloc(6),
                x => x.MatchLdarg(1),
                x => x.MatchGetVirt<CharacterBody>(nameof(CharacterBody.maxHealth)),
                x => x.MatchMul(),
                x => x.MatchStfld<DamageInfo>(nameof(DamageInfo.damage)));
            c.Index += 2;
            c.RemoveRange(2);
            c.Emit(OpCodes.Ldloc_S, (byte)5);
            c.EmitDelegate<Func<HealthComponent, float>>(GetEffectiveMaxHealthForFallDamage);
        }

        private static float GetEffectiveMaxHealthForFallDamage(HealthComponent healthComponent)
        {
            // Deal fall damage as % of health + shield instead of just health
            var output = healthComponent.fullCombinedHealth;

            if (healthComponent.shield > 0f)
            {
                // Slight reduction if the character has any shield
                output /= 1.04f;
            }

            return output;
        }
    }
}
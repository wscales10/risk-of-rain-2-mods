using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using UnityEngine;

namespace PactOfPunishment.Conditions
{
    public sealed class LastingConsequences : DefaultConditionDef
    {
        public override int MaxRank => 4;

        public override void Init()
        {
            IL.RoR2.HealthComponent.Heal += Utils.HookIL(HealthComponent_Heal);
            IL.RoR2.HealthComponent.ServerFixedUpdate += this.HealthComponent_ServerFixedUpdate;
            IL.RoR2.HealthComponent.AddBarrier += Utils.HookIL(HealthComponent_AddBarrier);
        }

        private void HealthComponent_AddBarrier(ILCursor c)
        {
            c.GotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<HealthComponent>(nameof(HealthComponent.barrier)),
                x => x.MatchLdarg(1),
                x => x.MatchAdd(),
                x => x.MatchLdarg(0),
                x => x.MatchCall<HealthComponent>($"get_{nameof(HealthComponent.fullBarrier)}"),
                x => x.MatchCall<Mathf>(nameof(Mathf.Min)),
                x => x.MatchCall<HealthComponent>($"set_{nameof(HealthComponent.Networkbarrier)}")
            );
            c.Index += 4;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<float, HealthComponent, float>>(this.ModifyHealing);
        }

        private void HealthComponent_ServerFixedUpdate(ILContext il)
        {
            var c = new ILCursor(il);

            c.GotoNext(MoveType.After,
                x => x.MatchLdloc(out _),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<HealthComponent>(nameof(HealthComponent.body)),
                x => x.MatchCallvirt<CharacterBody>($"get_{nameof(CharacterBody.maxShield)}"),
                x => x.MatchLdcR4(0.5f),
                x => x.MatchMul(),
                x => x.MatchLdarg(1),
                x => x.MatchMul());
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<float, HealthComponent, float>>(this.ModifyHealing);
        }

        private void HealthComponent_Heal(ILCursor c)
        {
            c.GotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<HealthComponent>(nameof(HealthComponent.health)),
                x => x.MatchLdloc(out _),
                x => x.MatchAdd(),
                x => x.MatchCall<HealthComponent>($"set_{nameof(HealthComponent.Networkhealth)}")
            );
            c.Index -= 2;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<float, HealthComponent, float>>(this.ModifyHealing);
        }

        private float ModifyHealing(float amount, HealthComponent self)
        {
            if (self.body.teamComponent.teamIndex == TeamIndex.Player)
            {
                float multiplier = 0.5f;

                int lastingConsequencesRank = this.GetRank(self);

                if (lastingConsequencesRank > 0)
                {
                    multiplier *= Mathf.Max(0, 1 - 0.25f * lastingConsequencesRank);
                }

                return amount * multiplier;
            }

            return amount;
        }
    }
}
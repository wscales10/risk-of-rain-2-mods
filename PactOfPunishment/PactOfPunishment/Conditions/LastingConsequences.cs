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
            IL.RoR2.HealthComponent.Heal += this.HealthComponent_Heal;
            IL.RoR2.HealthComponent.ServerFixedUpdate += this.HealthComponent_ServerFixedUpdate;
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

        private void HealthComponent_Heal(ILContext il)
        {
            var c = new ILCursor(il);

            c.GotoNext(MoveType.AfterLabel,
                x => x.MatchLdarg(1),
                x => x.MatchStloc(2),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<HealthComponent>(nameof(HealthComponent.health)),
                x => x.MatchLdarg(0),
                x => x.MatchCall<HealthComponent>($"get_{nameof(HealthComponent.fullHealth)}"),
                x => x.MatchBgeUn(out _));
            c.Emit(OpCodes.Ldarg_1);
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<float, HealthComponent, float>>(this.ModifyHealing);
            c.Emit(OpCodes.Starg_S, (byte)1);
        }

        private float ModifyHealing(float amount, HealthComponent self)
        {
            if (self.body.teamComponent.teamIndex == TeamIndex.Player)
            {
                int lastingConsequencesRank = this.GetRank(self);

                if (lastingConsequencesRank > 0)
                {
                    return amount * Mathf.Max(0, 1 - 0.25f * lastingConsequencesRank);
                }
            }

            return amount;
        }
    }
}
using RoR2;
using MonoMod.Cil;
using System;
using Mono.Cecil.Cil;

namespace PactOfPunishment.Conditions
{
    public sealed class RemoveOneShotProtection : DefaultConditionDef
    {
        public override int MaxRank => 1;

        public override void Init()
        {
            IL.RoR2.CharacterBody.RecalculateStats += this.CharacterBody_RecalculateStats;
        }

        private void CharacterBody_RecalculateStats(ILContext il)
        {
            var c = new ILCursor(il);
            c.GotoNext(x => x.MatchCall<CharacterBody>($"set_{nameof(CharacterBody.hasOneShotProtection)}"));
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<bool, CharacterBody, bool>>((value, self) => value && !(this.IsEnabled(self) && (self.teamComponent?.teamIndex) == TeamIndex.Player));
        }
    }
}
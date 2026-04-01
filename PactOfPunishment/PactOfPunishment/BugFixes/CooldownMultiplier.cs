using MonoMod.Cil;
using RoR2;
using UnityEngine;

namespace PactOfPunishment.BugFixes
{
    public class CooldownMultiplier : Module
    {
        public override void Init()
        {
            IL.RoR2.GenericSkill.CalculateFinalRechargeInterval += Utils.HookIL(this.GenericSkill_CalculateFinalRechargeInterval);
        }

        private void GenericSkill_CalculateFinalRechargeInterval(ILCursor c)
        {
            if (c.TryFindNext(out var cursors,
                x => x.MatchLdloc(0),
                x => x.MatchLdcR4(0.5f),
                x => x.MatchLdloc(0),
                x => x.MatchLdarg(0),
                x => x.MatchCall<GenericSkill>($"get_{nameof(GenericSkill.cooldownScale)}"),
                x => x.MatchMul(),
                x => x.MatchLdarg(0),
                x => x.MatchCall<GenericSkill>($"get_{nameof(GenericSkill.flatCooldownReduction)}"),
                x => x.MatchSub(),
                x => x.MatchCall<Mathf>(nameof(Mathf.Max)),
                x => x.MatchCall<Mathf>(nameof(Mathf.Min))))
            {
                var minInstr = cursors[10].Next; // instruction matched by the last predicate

                c.Index = cursors[10].Index;
                c.Remove(); // remove Mathf.Min

                c.Index = cursors[1].Index + 1; // after ldc.r4 0.5
                c.Emit(minInstr.OpCode, minInstr.Operand);
            }
            else
            {
                this.Logger.LogWarning("Couldn't find target for IL hook in GenericSkill.CalculateFinalRechargeInterval.");
            }
        }
    }
}

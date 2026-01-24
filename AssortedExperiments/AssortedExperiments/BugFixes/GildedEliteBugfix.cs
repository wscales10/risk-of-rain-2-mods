using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using UnityEngine;

namespace AssortedExperiments.BugFixes
{
    public class GildedEliteBugfix : Module
    {
        public override void Init()
        {
            IL.RoR2.AffixAurelioniteBehavior.OnServerDamageDealt += AffixAurelioniteBehavior_OnServerDamageDealt;
        }

        private static void AffixAurelioniteBehavior_OnServerDamageDealt(ILContext il)
        {
            var c = new ILCursor(il);
            c.GotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<AffixAurelioniteBehavior>(nameof(AffixAurelioniteBehavior.percentageOfGoldCopiedFromEnemy)),
                x => x.MatchLdloc(0),
                x => x.MatchGetVirt<CharacterMaster>(nameof(CharacterMaster.money)),
                x => x.MatchConvRUn(),
                x => x.MatchConvR4(),
                x => x.MatchMul(),
                x => x.MatchLdloc(2),
                x => x.MatchConvR4(),
                x => x.MatchCall<Mathf>(nameof(Mathf.Max)),
                x => x.MatchStloc(3));
            c.MoveAfterLabels();
            c.RemoveRange(10);
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_1);
            c.Emit(OpCodes.Ldloc_2);
            c.Emit(OpCodes.Ldloc_0);
            c.EmitDelegate<Func<AffixAurelioniteBehavior, DamageReport, int, CharacterMaster, float>>((self, damageReport, goldPerNugget, victimMaster) =>
                Mathf.Min(self.GetTotalPossibleNuggets(damageReport.damageDealt, damageReport.victimBody) * goldPerNugget, victimMaster.money));
        }
    }
}
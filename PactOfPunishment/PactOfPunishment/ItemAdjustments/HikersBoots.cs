using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;

namespace PactOfPunishment.ItemAdjustments
{
    public class HikersBoots : Module
    {
        public override void Init()
        {
            IL.RoR2.GlobalEventManager.ProcessHitEnemy += Utils.HookIL(GlobalEventManager_ProcessHitEnemy);
        }

        private static void GlobalEventManager_ProcessHitEnemy(ILCursor c)
        {
            c.RemoveMatch(
                x => x.MatchLdloc(out _),
                x => x.MatchLdsfld(typeof(DLC3Content.Buffs), nameof(DLC3Content.Buffs.CritChanceAndDamage)),
                x => x.MatchLdcR4(out _),
                x => x.MatchLdcI4(out _),
                x => x.MatchCallvirt<CharacterBody>(nameof(CharacterBody.SetTimedBuffDurationIfPresent))
            );

            int attackerBodyVariableNumber = -1, victimBodyVariableNumber = -1;
            ILLabel? label = null;
            c.RemoveMatch(
                x => x.MatchLdloc(out attackerBodyVariableNumber),
                x => x.MatchCallvirt<CharacterBody>($"get_{nameof(CharacterBody.corePosition)}"),
                x => x.MatchLdfld<UnityEngine.Vector3>(nameof(UnityEngine.Vector3.y)),
                x => x.MatchLdloc(out victimBodyVariableNumber),
                x => x.MatchCallvirt<CharacterBody>($"get_{nameof(CharacterBody.corePosition)}"),
                x => x.MatchLdfld<UnityEngine.Vector3>(nameof(UnityEngine.Vector3.y)),
                x => x.MatchSub(),
                x => x.MatchLdloc(out _),
                x => x.MatchCallvirt<CharacterBody>($"get_{nameof(CharacterBody.radius)}"),
                x => x.MatchBltUn(out label)
            );
            c.MoveAfterLabels();
            c.Emit(OpCodes.Ldloc_S, (byte)attackerBodyVariableNumber);
            c.Emit(OpCodes.Ldloc_S, (byte)victimBodyVariableNumber);
            c.EmitDelegate<Func<CharacterBody, CharacterBody, bool>>(IsAttackerAboveVictim);
            c.Emit(OpCodes.Brfalse_S, label);
        }

        private static bool IsAttackerAboveVictim(CharacterBody attackerBody, CharacterBody victimBody)
        {
            return attackerBody.footPosition.y > victimBody.corePosition.y;
        }
    }
}

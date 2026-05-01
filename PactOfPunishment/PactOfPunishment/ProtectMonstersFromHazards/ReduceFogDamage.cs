using MonoMod.Cil;
using RoR2;
using System;

namespace PactOfPunishment.ProtectMonstersFromHazards
{
    public class ReduceFogDamage : Module
    {
        public override void Init()
        {
            IL.RoR2.FogDamageController.MyFixedUpdate += Utils.HookIL(FogDamageController_MyFixedUpdate);
        }

        private static void FogDamageController_MyFixedUpdate(ILCursor c)
        {
            int bodyVariableNumber = -1;
            c.GotoNext(
                x => x.MatchLdloc(out bodyVariableNumber),
                x => x.MatchCallvirt<CharacterBody>($"get_{nameof(CharacterBody.IsDrone)}"),
                x => x.MatchBrtrue(out _));
            int index = c.Index;
            int damageVariableNumber = -1;
            c.GotoNext(
                x => x.MatchLdloc(out damageVariableNumber),
                x => x.MatchStfld<DamageInfo>(nameof(DamageInfo.damage)));
            c.Goto(index, MoveType.AfterLabel);

            c.EmitLdloc(damageVariableNumber);
            c.EmitLdloc(bodyVariableNumber);
            c.EmitLdarg(0);
            c.EmitDelegate<Func<float, CharacterBody, FogDamageController, float>>((damage, body, controller) =>
            {
                if (!body.isBoss)
                {
                    return damage;
                }

                var run = Run.instance as InfiniteTowerRun;

                if (run?.fogDamageController == controller && run?.waveController != null)
                {
                    return damage * 0.5f * (1 - 0.75f * run.waveController.zoneRadiusPercentage);
                }

                return damage;
            });
            c.EmitStloc(damageVariableNumber);
        }
    }
}
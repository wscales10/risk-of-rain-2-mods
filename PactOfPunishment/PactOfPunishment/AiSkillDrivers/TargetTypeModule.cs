using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2.CharacterAI;
using System;
using System.Collections.Generic;
using System.Text;

namespace PactOfPunishment.AiSkillDrivers
{
    public class TargetTypeModule : Module
    {
        public override void Init()
        {
            IL.RoR2.CharacterAI.BaseAI.EvaluateSingleSkillDriver += Utils.HookIL(BaseAI_EvaluateSingleSkillDriver);
        }

        private static void BaseAI_EvaluateSingleSkillDriver(ILCursor c)
        {
            int targetVariableNumber = 1;
            int targetTypeVariableNumber = 2;
            int aiSkillDriverArgumentNumber = 2;
            c.GotoNext(MoveType.After,
                x => x.MatchLdloc(targetTypeVariableNumber),
                x => x.MatchSwitch(out _),
                x => x.MatchBr(out _));
            c.Index--;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitLdarg(aiSkillDriverArgumentNumber);
            c.EmitLdloc(targetTypeVariableNumber);
            c.EmitDelegate<Func<BaseAI, AISkillDriver, AISkillDriver.TargetType, BaseAI.Target?>>((ai, aiSkillDriver, targetType) =>
            {
                if (ModdedTargetType.TryGetTarget(ai, aiSkillDriver, out var target))
                {
                    return target;
                }

                return null;
            });
            c.EmitStloc(targetVariableNumber);
        }
    }
}

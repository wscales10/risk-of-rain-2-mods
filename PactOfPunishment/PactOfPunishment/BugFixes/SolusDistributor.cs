using EntityStates.MinePod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace PactOfPunishment.BugFixes
{
    public class SolusDistributor : Module
    {
        public override void Init()
        {
            IL.EntityStates.MinePod.MinePlant.OnEnter += Utils.HookIL(ScaleMineSpawnDelayWithAttackSpeed);
            IL.EntityStates.MinePod.MinePlant.FixedUpdate += Utils.HookIL(ScaleMineSpawnDelayWithAttackSpeed);
        }

        private static void ScaleMineSpawnDelayWithAttackSpeed(ILCursor c)
        {
            while (c.TryGotoNext(MoveType.After, x => x.MatchLdsfld<MinePlant>(nameof(MinePlant.mineSpawnDelay))))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<float, MinePlant, float>>((orig, self) => orig / self.attackSpeedStat);
            }
        }
    }
}
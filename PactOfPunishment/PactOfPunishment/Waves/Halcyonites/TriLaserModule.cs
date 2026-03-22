using EntityStates;
using EntityStates.Halcyonite;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using UnityEngine;

namespace PactOfPunishment.Waves.Halcyonites
{
    public class TriLaserModule : Module
    {
        public override void Init()
        {
            IL.EntityStates.Halcyonite.TriLaser.FixedUpdate += Utils.HookIL(TriLaser_FixedUpdate);
            IL.EntityStates.Halcyonite.TriLaser.OnEnter += Utils.HookIL(UseBaseDurationIfScalingWithAttackSpeed);
            On.EntityStates.Halcyonite.TriLaser.OnEnter += this.TriLaser_OnEnter;
            IL.EntityStates.Halcyonite.TriLaser.FireTriLaser += Utils.HookIL(UseBaseDurationIfScalingWithAttackSpeed);
            IL.EntityStates.Halcyonite.TriLaser.FireTriLaser += Utils.HookIL(MultiplyDamage);
            IL.EntityStates.Halcyonite.ChargeTriLaser.OnEnter += Utils.HookIL(ChargeTriLaser_OnEnter);
        }

        private static void MultiplyDamage(ILCursor c)
        {
            while (c.TryGotoNext(MoveType.After, x => x.MatchLdsfld<TriLaser>(nameof(TriLaser.damageCoefficient))))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<float, TriLaser, float>>((orig, self) => GetCustomStats(self).DamageMultiplier * orig);
            }
        }

        private static void ChargeTriLaser_OnEnter(ILCursor c)
        {
            c.GotoNext(x => x.MatchStfld<ChargeTriLaser>(nameof(ChargeTriLaser.duration)));
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<float, ChargeTriLaser, float>>((orig, self) => orig * GetCustomStats(self).ChargeTimeMultiplier);
        }

        private static void TriLaser_FixedUpdate(ILCursor c)
        {
            c.GotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<TriLaser>(nameof(TriLaser.timesFired)),
                x => x.MatchLdcI4(3),
                x => x.MatchBge(out _));
            c.Index += 2;
            c.Remove();
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<TriLaser, int>>(self => GetCustomStats(self).GetTotalTimesToFire(self, true));

            c.GotoLast(
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<TriLaser>(nameof(TriLaser.timesFired)),
                x => x.MatchLdcI4(2),
                x => x.MatchBle(out _));
            c.Index += 2;
            c.Remove();
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<TriLaser, int>>(self => GetCustomStats(self).GetTotalTimesToFire(self, true) - 1);
        }

        private static TriLaserStats GetCustomStats(BaseState self)
        {
            var body = self.characterBody;
            return body && body.TryGetComponent<StateModifier>(out var overrideComponent) ? overrideComponent.Stats : new TriLaserStats();
        }

        private void UseBaseDurationIfScalingWithAttackSpeed(ILCursor c)
        {
            while (c.TryGotoNext(x => x.MatchStfld<TriLaser>(nameof(TriLaser.duration))))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<float, TriLaser, float>>((orig, self) =>
                {
                    var customStats = GetCustomStats(self);
                    var duration = customStats.ScaleWithAttackSpeed ? TriLaser.baseDuration : orig;

                    var durationFirstHalf = duration / 2;
                    var durationSecondHalf = duration / 2;

                    if (customStats.FireCooldownOverride.HasValue)
                    {
                        durationFirstHalf = self.fireCooldown * (customStats.GetTotalTimesToFire(self, false) - 1);
                    }

                    if (customStats.EndLagOverride.HasValue)
                    {
                        durationSecondHalf *= customStats.EndLagOverride.Value;
                    }

                    duration = durationFirstHalf + durationSecondHalf;

                    if (!Mathf.Approximately(duration, self.duration))
                    {
                        this.Logger.LogDebug($"Changing duration from {self.duration} to {duration}");
                    }

                    return duration;
                });
                c.Index++;
            }
        }

        private void TriLaser_OnEnter(On.EntityStates.Halcyonite.TriLaser.orig_OnEnter orig, TriLaser self)
        {
            orig(self);
            var customStats = GetCustomStats(self);
            float fireCooldown = self.fireCooldown;

            if (customStats.FireCooldownOverride.HasValue)
            {
                fireCooldown *= customStats.FireCooldownOverride.Value / 0.5f;
            }
            else
            {
                fireCooldown *= 2f / (customStats.GetTotalTimesToFire(self, false) - 1);
            }

            this.Logger.LogDebug($"Changing fire cooldown from {self.fireCooldown} to {fireCooldown}");
            self.fireCooldown = fireCooldown;
        }

        [RequireComponent(typeof(CharacterBody))]
        public class StateModifier : MonoBehaviour
        {
            public readonly TriLaserStats Stats = new TriLaserStats();
        }

        public class TriLaserStats
        {
            public int BaseTotalTimesToFire = 3;

            public bool ScaleWithAttackSpeed = false;

            public float ChargeTimeMultiplier = 1;

            public float? FireCooldownOverride;

            public float? EndLagOverride;

            public float DamageMultiplier = 1;

            public bool KeepFiringWhileKeyDown;

            public int GetTotalTimesToFire(BaseState state, bool duringFixedUpdate)
            {
                if (duringFixedUpdate && this.KeepFiringWhileKeyDown && state.inputBank.skill2.down)
                {
                    return int.MaxValue;
                }

                return this.ScaleWithAttackSpeed ? (int)(this.BaseTotalTimesToFire * HalcyoniteModule.GetAttackSpeedStat(state)) : this.BaseTotalTimesToFire;
            }
        }
    }
}
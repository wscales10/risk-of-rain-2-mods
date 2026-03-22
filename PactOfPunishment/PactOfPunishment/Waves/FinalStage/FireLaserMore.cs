using EntityStates;
using EntityStates.PrimeMeridian;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PactOfPunishment.Waves.FinalStage
{
    public class FireLaserMore : Module
    {
        public override void Init()
        {
            IL.EntityStates.PrimeMeridian.LunarGazeLaserEnd.FixedUpdate += Utils.HookIL(LunarGazeLaserEnd_FixedUpdate);
            On.EntityStates.PrimeMeridian.LunarGazeLaserFire.OnEnter += this.LunarGazeLaserFire_OnEnter;
            On.EntityStates.PrimeMeridian.LunarGazeLaserCharge.OnEnter += this.LunarGazeLaserCharge_OnEnter;
            On.EntityStates.PrimeMeridian.LunarGazeLaserFire.OnExit += this.LunarGazeLaserFire_OnExit;

            IL.EntityStates.PrimeMeridian.LunarGazeLaserCharge.FixedUpdate += Utils.HookIL(InterceptGetChargeDuration);
            IL.EntityStates.PrimeMeridian.LunarGazeLaserCharge.OnEnter += Utils.HookIL(InterceptGetChargeDuration);
            IL.EntityStates.PrimeMeridian.LunarGazeLaserCharge.Update += Utils.HookIL(InterceptGetChargeDuration);
            IL.EntityStates.PrimeMeridian.LunarGazeLaserCharge.UpdateEyeColor += Utils.HookIL(InterceptGetChargeDuration);
        }

        private static void InterceptGetChargeDuration(ILCursor c)
        {
            c.Index = 0;

            while (c.TryGotoNext(MoveType.AfterLabel, x => x.MatchLdsfld<LunarGazeLaserCharge>(nameof(LunarGazeLaserCharge.duration))))
            {
                c.Remove();
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<LunarGazeLaserCharge, float>>(state =>
                {
                    if (!state.TryGetComponent<FireLaserMoreBehavior>(out var behavior))
                    {
                        return LunarGazeLaserCharge.duration;
                    }

                    return behavior.ChargeDuration;
                });
            }
        }

        private static void LunarGazeLaserEnd_FixedUpdate(ILCursor c)
        {
            c.GotoNext(MoveType.AfterLabel,
                x => x.MatchLdsfld<LunarGazeLaserEnd>(nameof(LunarGazeLaserEnd.fireEndDelayDuration)));
            c.Remove();
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<LunarGazeLaserEnd, float>>(state =>
            {
                if (!state.TryGetComponent<FireLaserMoreBehavior>(out var behavior))
                {
                    return LunarGazeLaserEnd.fireEndDelayDuration;
                }

                return behavior.FireEndDelayDuration;
            });

            c.GotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<EntityState>(nameof(EntityState.outer)),
                x => x.MatchCallvirt<EntityStateMachine>(nameof(EntityStateMachine.SetNextStateToMain)));
            c.Index += 2;
            c.Remove();
            c.EmitDelegate<Action<EntityStateMachine>>(self =>
            {
                if (!self.TryGetComponent<FireLaserMoreBehavior>(out var behavior))
                {
                    self.SetNextStateToMain();
                    return;
                }

                if (behavior.isDoingMiniBurst || behavior.chargeStateType is null)
                {
                    behavior.isDoingMiniBurst = false;
                    self.SetNextStateToMain();
                    return;
                }

                behavior.isDoingMiniBurst = true;
                self.SetNextState(EntityStateCatalog.InstantiateState(behavior.chargeStateType));
            });
        }

        private void LunarGazeLaserFire_OnExit(On.EntityStates.PrimeMeridian.LunarGazeLaserFire.orig_OnExit orig, LunarGazeLaserFire self)
        {
            if (self.TryGetComponent<FireLaserMoreBehavior>(out var behavior))
            {
                behavior.CacheLaserTargets(self.laserTargets);
            }

            orig(self);
        }

        private void LunarGazeLaserCharge_OnEnter(On.EntityStates.PrimeMeridian.LunarGazeLaserCharge.orig_OnEnter orig, LunarGazeLaserCharge self)
        {
            if (self.TryGetComponent<FireLaserMoreBehavior>(out var behavior))
            {
                behavior.chargeStateType = self.GetType();
            }

            orig(self);
        }

        private void LunarGazeLaserFire_OnEnter(On.EntityStates.PrimeMeridian.LunarGazeLaserFire.orig_OnEnter orig, LunarGazeLaserFire self)
        {
            if (self.TryGetComponent<FireLaserMoreBehavior>(out var behavior))
            {
                float newLockOnDuration;
                float newFireDuration;
                float newWindDownDuration;

                if (behavior.isDoingMiniBurst)
                {
                    newLockOnDuration = 0.5f;
                    newFireDuration = 1.5f;
                    newWindDownDuration = self.windDownDuration / 2f;
                }
                else
                {
                    newLockOnDuration = self.lockOnDelayDuration;
                    newFireDuration = self.duration - (self.lockOnDelayDuration + self.windDownDuration);
                    newWindDownDuration = 1.25f;
                }

                self.duration = newLockOnDuration + newFireDuration + newWindDownDuration;
                self.lockOnDelayDuration = newLockOnDuration;
                self.windDownDuration = newWindDownDuration;
            }

            orig(self);

            if (self.TryGetComponent<FireLaserMoreBehavior>(out behavior))
            {
                behavior.InitLaserTargets(self.laserTargets);
            }
        }

        public class FireLaserMoreBehavior : MonoBehaviour
        {
            public Type? chargeStateType;

            public bool isDoingMiniBurst;

            private readonly Dictionary<CharacterBody, Vector3?> lockOnPositions = new Dictionary<CharacterBody, Vector3?>();

            public float ChargeDuration => this.isDoingMiniBurst ? 1.5f : LunarGazeLaserCharge.duration;

            public float FireEndDelayDuration => this.isDoingMiniBurst ? LunarGazeLaserEnd.fireEndDelayDuration : 0.4f;

            public void CacheLaserTargets(List<LunarGazeLaserFire.LaserTargetInfo> laserTargets)
            {
                this.lockOnPositions.Clear();

                foreach (var laserTargetInfo in laserTargets)
                {
                    this.lockOnPositions[laserTargetInfo.body] = laserTargetInfo.lastLockedOnPosition;
                }
            }

            public void InitLaserTargets(List<LunarGazeLaserFire.LaserTargetInfo> laserTargets)
            {
                foreach (var laserTargetInfo in laserTargets)
                {
                    if (this.lockOnPositions.TryGetValue(laserTargetInfo.body, out var position) && position.HasValue)
                    {
                        laserTargetInfo.lastLockedOnPosition = position.Value;
                    }
                }
            }
        }
    }
}
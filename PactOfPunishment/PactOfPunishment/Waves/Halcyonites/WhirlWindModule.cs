using EntityStates;
using EntityStates.Halcyonite;
using HG;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static PactOfPunishment.Waves.Halcyonites.WhirlWindModule;

namespace PactOfPunishment.Waves.Halcyonites
{
    public interface IOverrideGetTarget<TResult, TArgs>
    {
        IEnumerator<OrigResult<TResult>>? CurrentEnumerator { get; }

        TResult Result { get; }

        void StartLoop(TArgs args);
    }

    public interface IOverrideHasTarget<TResult, TArgs>
    {
        TResult GetResult(TResult orig, TArgs args);
    }

    public interface IOverrideTargetPos : IOnOff
    {
        Vector3 TargetPos { get; }

        Vector3 TargetMoveDirection { get; }

        void OnDashStart(WhirlWindPersuitCycle state);

        bool CheckIfArrived(bool orig);

        void Update();

        void Reset();
    }

    public partial class WhirlWindModule : Module
    {
        public override void Init()
        {
            // TODO: similar to TriLaserModule.
            /* Some wants:
             * can target position without body
             * can use airNodes to navigate
             * can move towards target without LoS
             * implementations which allow things like using the nodeGraph to move into the safe zone, going to last seen target position if there is no LoS (not the same as last bullseye position I guess)
             * At the moment it's not working amazingly. I need one bool flag to make all the logic visible in-game so I can see where the Halcyonite is trying to get to and how it's trying to get there.
             */

            On.EntityStates.Halcyonite.WhirlWindPersuitCycle.StartDash += this.WhirlWindPersuitCycle_StartDash;
            On.EntityStates.Halcyonite.WhirlWindPersuitCycle.UpdateDash += this.WhirlWindPersuitCycle_UpdateDash;
            On.EntityStates.Halcyonite.WhirlWindPersuitCycle.OnExit += this.WhirlWindPersuitCycle_OnExit;
            On.EntityStates.Halcyonite.WhirlWindPersuitCycle.CheckIfArrived += this.WhirlWindPersuitCycle_CheckIfArrived;

            IL.EntityStates.Halcyonite.WhirlWindPersuitCycle.CheckIfArrived += InterceptGetTargetPos(true).HookIL();
            IL.EntityStates.Halcyonite.WhirlWindPersuitCycle.UpdateDash += InterceptGetTargetPos(true).HookIL();
            IL.EntityStates.Halcyonite.WhirlWindPersuitCycle.StartDash += InterceptGetTargetPos(false).HookIL();
            IL.EntityStates.Halcyonite.WhirlWindPersuitCycle.UpdateFindTarget += InterceptGetTargetPos(false).HookIL();

            IL.EntityStates.Halcyonite.WhirlWindPersuitCycle.UpdateFindTarget += Utils.HookIL(WhirlWindPersuitCycle_UpdateFindTarget);
        }

        private static bool CheckIfArrived(WhirlWindPersuitCycle self)
        {
            bool orig = Vector3.Dot(self.targetPos - self.transform.position, GetTargetMoveDirection(self)) < 0f;

            if (self.TryGetComponentWhere<IOverrideTargetPos>(x => x.CustomEnabled, out var component))
            {
                return component.CheckIfArrived(orig);
            }
            else
            {
                return orig;
            }
        }

        private static Action<ILCursor> InterceptGetTargetPos(bool moveDirectionOnly) => c => InterceptGetTargetPosInternal(c, moveDirectionOnly);

        private static Vector3 GetTargetPos(WhirlWindPersuitCycle self)
        {
            if (self.TryGetComponentWhere<IOverrideTargetPos>(x => x.CustomEnabled, out var component))
            {
                return component.TargetPos;
            }
            else
            {
                return self.targetPos;
            }
        }

        private static Vector3 GetTargetMoveDirection(WhirlWindPersuitCycle self)
        {
            if (self.TryGetComponentWhere<IOverrideTargetPos>(x => x.CustomEnabled, out var component))
            {
                return component.TargetMoveDirection;
            }
            else
            {
                return self.targetMoveDirt;
            }
        }

        private static Vector3 GetNormalizedTargetMoveDirection(WhirlWindPersuitCycle self)
        {
            if (self.TryGetComponentWhere<IOverrideTargetPos>(x => x.CustomEnabled, out var component))
            {
                return component.TargetMoveDirection.normalized;
            }
            else
            {
                return self.targetMoveDirt.normalized;
            }
        }

        private static void InterceptGetTargetPosInternal(ILCursor c, bool moveDirectionOnly)
        {
            if (!moveDirectionOnly)
            {
                c.InterceptLoadField<WhirlWindPersuitCycle, Vector3>(nameof(WhirlWindPersuitCycle.targetPos), GetTargetPos);
            }

            c.Index = 0;
            c.InterceptLoadField<WhirlWindPersuitCycle, Vector3>(nameof(WhirlWindPersuitCycle.targetMoveDirt), GetTargetMoveDirection);

            c.Index = 0;
            while (c.TryGotoNext(MoveType.AfterLabel,
                x => x.MatchLdflda<WhirlWindPersuitCycle>(nameof(WhirlWindPersuitCycle.targetMoveDirt)),
                x => x.MatchCall<Vector3>($"get_{nameof(Vector3.normalized)}")))
            {
                c.RemoveRange(2);
                c.MoveAfterLabels(); // AfterLabel stuff is probably not needed here, but just to be safe...
                c.EmitDelegate<Func<WhirlWindPersuitCycle, Vector3>>(GetNormalizedTargetMoveDirection);
            }
        }

        private static void WhirlWindPersuitCycle_UpdateFindTarget(ILCursor c)
        {
            // Intercept usage of targetBody to determine whether we have a target
            c.GotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<WhirlWindPersuitCycle>(nameof(WhirlWindPersuitCycle.targetBody)),
                x => x.MatchCall<UnityEngine.Object>("op_Implicit"),
                x => x.MatchBrtrue(out _)
            );
            c.Index--;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<bool, WhirlWindPersuitCycle, bool>>((orig, self) =>
            {
                if (self.TryGetComponent<IOverrideHasTarget<bool, WhirlWindPersuitCycle>>(out var component))
                {
                    return component.GetResult(orig, self);
                }

                return orig;
            });

            // If does not have target...
            c.Index++;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<WhirlWindPersuitCycle, bool>>(self =>
            {
                bool hasComponent = false;

                foreach (var component in self.gameObject.GetComponents<IOverrideGetTarget<WhirlWindTargetInfo, WhirlWindPersuitCycle>>())
                {
                    hasComponent = true;
                    component.StartLoop(self);
                }

                return hasComponent;
            });
            ILLabel origLabel = c.DefineLabel();
            c.Emit(OpCodes.Brfalse_S, origLabel);
            ILLabel loopStartLabel = c.MarkLabel();
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<WhirlWindPersuitCycle, bool>>(self =>
            {
                bool isAnyComponentUnfinished = false;

                foreach (var component in self.gameObject.GetComponents<IOverrideGetTarget<WhirlWindTargetInfo, WhirlWindPersuitCycle>>())
                {
                    isAnyComponentUnfinished |= component.CurrentEnumerator!.MoveNext();
                }

                return isAnyComponentUnfinished;
            });
            ILLabel postLoopLabel = c.DefineLabel();
            c.Emit(OpCodes.Brfalse_S, postLoopLabel);
            c.MarkLabel(origLabel);
            c.GotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdarg(0),
                x => x.MatchCall<EntityState>($"get_{nameof(EntityState.characterDirection)}"),
                x => x.MatchCallvirt<CharacterDirection>($"get_{nameof(CharacterDirection.forward)}"),
                x => x.MatchStfld<WhirlWindPersuitCycle>(nameof(WhirlWindPersuitCycle.startForwardDirt)),
                x => x.MatchRet() // TODO: use IL_0114 instead of this?
            );
            c.Index--;
            ILLabel loopEndLabel = c.MarkLabel();
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<WhirlWindPersuitCycle, bool>>(self =>
            {
                bool hasComponent = false;

                foreach (var component in self.gameObject.GetComponents<IOverrideGetTarget<WhirlWindTargetInfo, WhirlWindPersuitCycle>>())
                {
                    hasComponent = true;
                    component.CurrentEnumerator!.Current.Value = new WhirlWindTargetInfo { body = self.targetBody, pos = self.targetPos };
                }

                return hasComponent;
            });
            c.Emit(OpCodes.Brtrue_S, loopStartLabel);
            c.Emit(OpCodes.Ret);
            c.MarkLabel(postLoopLabel);
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<WhirlWindPersuitCycle>>(self =>
            {
                foreach (var targetInfo in self.gameObject.GetComponents<IOverrideGetTarget<WhirlWindTargetInfo, WhirlWindPersuitCycle>>().Select(x => x.Result))
                {
                    self.targetBody = targetInfo.body;
                    self.targetPos = targetInfo.pos;
                }
            });
        }

        private void WhirlWindPersuitCycle_CheckIfArrived(On.EntityStates.Halcyonite.WhirlWindPersuitCycle.orig_CheckIfArrived orig, WhirlWindPersuitCycle self)
        {
            // TODO: this is temporary, I want to use an IL hook but this will help with debugging
            if (CheckIfArrived(self))
            {
                self.state = WhirlWindPersuitCycle.PersuitState.Decelerate;
                self.startDecelerateTimeStamp = self.fixedAge;
                self.startedDash = false;
            }
        }

        private void WhirlWindPersuitCycle_OnExit(On.EntityStates.Halcyonite.WhirlWindPersuitCycle.orig_OnExit orig, WhirlWindPersuitCycle self)
        {
            if (self.TryGetComponentWhere<IOverrideTargetPos>(x => x.CustomEnabled, out var component))
            {
                component.Reset();
            }

            orig(self);
        }

        private void WhirlWindPersuitCycle_UpdateDash(On.EntityStates.Halcyonite.WhirlWindPersuitCycle.orig_UpdateDash orig, WhirlWindPersuitCycle self)
        {
            if (self.TryGetComponentWhere<IOverrideTargetPos>(x => x.CustomEnabled, out var component))
            {
                component.Update();
            }

            orig(self);
        }

        private void WhirlWindPersuitCycle_StartDash(On.EntityStates.Halcyonite.WhirlWindPersuitCycle.orig_StartDash orig, WhirlWindPersuitCycle self)
        {
            if (self.TryGetComponentWhere<IOverrideTargetPos>(x => x.CustomEnabled, out var component))
            {
                component.OnDashStart(self);
            }

            orig(self);
        }

        public struct WhirlWindTargetInfo
        {
            public CharacterBody? body;

            public Vector3 pos;
        }

        public class OrigResult<T>
        {
            public T Value;
        }

        public abstract class OverrideGetTarget<T, TArgs> : MonoBehaviour, IOverrideGetTarget<T, TArgs>
            where T : struct
        {
            public IEnumerator<OrigResult<T>>? CurrentEnumerator { get; private set; }

            public T Result { get; protected set; }

            public void StartLoop(TArgs args)
            {
                this.CurrentEnumerator = this.Foo(args).GetEnumerator();
            }

            protected virtual IEnumerable<OrigResult<T>> Foo(TArgs args)
            {
                yield return this.Orig(out var orig);
                this.Result = orig.Value;
            }

            protected OrigResult<T> Orig(out OrigResult<T> result)
            {
                return result = new OrigResult<T>();
            }
        }
    }
}
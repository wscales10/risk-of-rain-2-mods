using EntityStates;
using EntityStates.Halcyonite;
using HG;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.CharacterAI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PactOfPunishment.Waves.Halcyonites
{
    public class WhirlWindModule : Module
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

        public interface IOverrideTargetPos
        {
            Vector3 TargetPos { get; }

            Vector3 TargetMoveDirection { get; }

            void OnDashStart(WhirlWindPersuitCycle state);

            void Update();

            void Reset();
        }

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

            IL.EntityStates.Halcyonite.WhirlWindPersuitCycle.CheckIfArrived += InterceptGetTargetPos(true).HookIL();
            IL.EntityStates.Halcyonite.WhirlWindPersuitCycle.UpdateDash += InterceptGetTargetPos(true).HookIL();
            IL.EntityStates.Halcyonite.WhirlWindPersuitCycle.StartDash += InterceptGetTargetPos(false).HookIL();
            IL.EntityStates.Halcyonite.WhirlWindPersuitCycle.UpdateFindTarget += InterceptGetTargetPos(false).HookIL();

            IL.EntityStates.Halcyonite.WhirlWindPersuitCycle.UpdateFindTarget += Utils.HookIL(WhirlWindPersuitCycle_UpdateFindTarget);
        }

        private static Action<ILCursor> InterceptGetTargetPos(bool moveDirectionOnly) => c => InterceptGetTargetPosInternal(c, moveDirectionOnly);

        private static void InterceptGetTargetPosInternal(ILCursor c, bool moveDirectionOnly)
        {
            if (!moveDirectionOnly)
            {
                c.InterceptLoadField<WhirlWindPersuitCycle, Vector3>(nameof(WhirlWindPersuitCycle.targetPos), self =>
                {
                    if (self.TryGetComponent<IOverrideTargetPos>(out var component))
                    {
                        return component.TargetPos;
                    }
                    else
                    {
                        return self.targetPos;
                    }
                });
            }

            c.Index = 0;
            c.InterceptLoadField<WhirlWindPersuitCycle, Vector3>(nameof(WhirlWindPersuitCycle.targetMoveDirt), self =>
            {
                if (self.TryGetComponent<IOverrideTargetPos>(out var component))
                {
                    return component.TargetMoveDirection;
                }
                else
                {
                    return self.targetMoveDirt;
                }
            });

            c.Index = 0;
            while (c.TryGotoNext(MoveType.AfterLabel,
                x => x.MatchLdflda<WhirlWindPersuitCycle>(nameof(WhirlWindPersuitCycle.targetMoveDirt)),
                x => x.MatchCall<Vector3>($"get_{nameof(Vector3.normalized)}")))
            {
                c.RemoveRange(2);
                c.MoveAfterLabels(); // AfterLabel stuff is probably not needed here, but just to be safe...
                c.EmitDelegate<Func<WhirlWindPersuitCycle, Vector3>>(self =>
                {
                    if (self.TryGetComponent<IOverrideTargetPos>(out var component))
                    {
                        return component.TargetMoveDirection.normalized;
                    }
                    else
                    {
                        return self.targetMoveDirt.normalized;
                    }
                });
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

        private void WhirlWindPersuitCycle_OnExit(On.EntityStates.Halcyonite.WhirlWindPersuitCycle.orig_OnExit orig, WhirlWindPersuitCycle self)
        {
            if (self.TryGetComponent<IOverrideTargetPos>(out var component))
            {
                component.Reset();
            }

            orig(self);
        }

        private void WhirlWindPersuitCycle_UpdateDash(On.EntityStates.Halcyonite.WhirlWindPersuitCycle.orig_UpdateDash orig, WhirlWindPersuitCycle self)
        {
            if (self.TryGetComponent<IOverrideTargetPos>(out var component))
            {
                component.Update();
            }

            orig(self);
        }

        private void WhirlWindPersuitCycle_StartDash(On.EntityStates.Halcyonite.WhirlWindPersuitCycle.orig_StartDash orig, WhirlWindPersuitCycle self)
        {
            if (self.TryGetComponent<IOverrideTargetPos>(out var component))
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

        public class OverrideGetTarget : OverrideGetTarget<WhirlWindTargetInfo, WhirlWindPersuitCycle>, IOverrideHasTarget<bool, WhirlWindPersuitCycle>
        {
            public bool GetResult(bool orig, WhirlWindPersuitCycle args)
            {
                return orig || args.targetPos != default;
            }

            protected override IEnumerable<OrigResult<WhirlWindTargetInfo>> Foo(WhirlWindPersuitCycle args)
            {
                yield return this.Orig(out var orig);
                if (orig.Value.body is null)
                {
                    if (!Utils.IsSafeLocation(args.characterBody.corePosition) && Run.instance is InfiniteTowerRun run && run.safeWardController)
                    {
                        this.Result = new WhirlWindTargetInfo
                        {
                            pos = GetTargetPosition(run.safeWardController.transform.position)
                        };
                        yield break;
                    }

                    if (args.characterBody.master.TryGetComponent<BaseAI>(out var ai) && ai.currentEnemy?.lastKnownBullseyePosition != null)
                    {
                        this.Result = new WhirlWindTargetInfo
                        {
                            body = ai.currentEnemy!.characterBody,
                            pos = GetTargetPosition(ai.currentEnemy.lastKnownBullseyePosition.Value),
                        };
                        yield break;
                    }
                }

                this.Result = orig.Value;

                Vector3 GetTargetPosition(Vector3 position)
                {
                    return position + (args.transform.position - position).normalized * 2f;
                }
            }
        }

        public class UseAirNodes : MonoBehaviour, IOverrideTargetPos
        {
            private Instance? instance;

            private LineRenderer lineRenderer;

            private EntityStateMachine weaponStateMachine;

            private GameObject debugSphere;

            Vector3 IOverrideTargetPos.TargetPos => this.Instance1.currentTargetPosition;

            Vector3 IOverrideTargetPos.TargetMoveDirection => this.Instance1.currentTargetMoveDirection;

            private EntityStateMachine.CommonComponentCache CommonComponents => this.weaponStateMachine.commonComponents;

            private Instance? Instance1
            {
                get => this.instance;

                set
                {
                    this.instance = value;

                    var positions = value?.path?.Where(x => x.HasValue).Select(x => x!.Value).ToArray() ?? Array.Empty<Vector3>();
                    this.lineRenderer.positionCount = positions.Length;
                    this.lineRenderer.SetPositions(positions);
                }
            }

            public void Update()
            {
                if (this.Instance1 is Instance instance)
                {
                    this.debugSphere.transform.position = instance.currentTargetPosition;
                }
            }

            public void OnEnable()
            {
                this.debugSphere.SetActive(true);
            }

            public void OnDisable()
            {
                this.debugSphere.SetActive(false);
            }

            public void OnDestroy()
            {
                Destroy(this.debugSphere);
                Destroy(this.lineRenderer);
            }

            public Vector3 GetCurrentPosition(WhirlWindPersuitCycle state) => state.characterBody.corePosition;

            public void Awake()
            {
                this.lineRenderer = Stage.instance.gameObject.AddComponent<LineRenderer>();
                this.lineRenderer.material = DebugOverlay.defaultWireMaterial;
                this.weaponStateMachine = EntityStateMachine.FindByCustomName(this.gameObject, "Weapon");
                this.debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                this.debugSphere.transform.localScale = Vector3.one * 0.5f;
            }

            public void OnDashStart(WhirlWindPersuitCycle state) => this.Instance1 = new Instance(this.GetPath(state), state, this.GetCurrentPosition);

            void IOverrideTargetPos.Reset() => this.Instance1 = default;

            void IOverrideTargetPos.Update() => this.Instance1?.Update();

            private Path? GetPath(WhirlWindPersuitCycle state)
            {
                var path = new Path(SceneInfo.instance.airNodes);
                var pathTask = SceneInfo.instance.airNodes.ComputePath(new RoR2.Navigation.NodeGraph.PathRequest
                {
                    path = path,
                    startPos = this.GetCurrentPosition(state),
                    endPos = state.targetPos,
                    hullClassification = this.CommonComponents.characterBody.hullClassification,
                    maxJumpHeight = float.MaxValue,
                    maxSpeed = WhirlWindPersuitCycle.dashSpeedCoefficient,
                });
                pathTask?.Wait();
                return pathTask?.wasReachable == true ? path : null;
            }

            private class Instance
            {
                public Vector3?[]? path;

                public int indexInPath;

                public Vector3 currentTargetPosition;

                public Vector3 currentTargetMoveDirection;

                private readonly WhirlWindPersuitCycle state;

                private readonly Func<Vector3> getCurrentPosition;

                public Instance(Path? path, WhirlWindPersuitCycle state, Func<WhirlWindPersuitCycle, Vector3> getCurrentPosition)
                {
                    this.path = path is null ? null : Enumerable.Range(0, path.waypointsCount).Select<int, Vector3?>(i =>
                    {
                        if (SceneInfo.instance.airNodes.GetNodePosition(path[i].nodeIndex, out var position))
                        {
                            return position;
                        }
                        else
                        {
                            return null;
                        }
                    }).ToArray();
                    this.state = state;
                    this.getCurrentPosition = () => getCurrentPosition(this.state);
                    this.UpdateCurrentTargetPosition();
                }

                public void Update()
                {
                    if (this.path is null)
                    {
                        return;
                    }

                    for (int i = this.path.Length - 1; i > this.indexInPath; i--)
                    {
                        if (this.path[i] is Vector3 nodePosition && BaseAI.CheckLoS(this.getCurrentPosition(), nodePosition))
                        {
                            this.indexInPath = i;
                            this.UpdateCurrentTargetPosition();
                            return;
                        }
                    }

                    if (this.indexInPath < this.path?.Length && (this.currentTargetPosition - this.getCurrentPosition()).sqrMagnitude < 4)
                    {
                        this.indexInPath++;
                        this.UpdateCurrentTargetPosition();
                    }
                }

                public void UpdateCurrentTargetPosition()
                {
                    if (this.path is null || this.path.Length == 0)
                    {
                        this.currentTargetPosition = this.getCurrentPosition();
                        this.currentTargetMoveDirection = this.state.characterDirection.moveVector;
                        return;
                    }

                    if (this.path[Mathf.Clamp(this.indexInPath, 0, this.path.Length - 1)] is Vector3 position)
                    {
                        this.currentTargetPosition = position;
                        this.currentTargetMoveDirection = (this.currentTargetPosition - this.getCurrentPosition()).normalized;
                    }
                }
            }
        }
    }
}
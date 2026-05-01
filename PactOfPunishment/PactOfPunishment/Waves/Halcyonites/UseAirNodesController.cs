using EntityStates.Halcyonite;
using HG;
using PactOfPunishment.ProtectMonstersFromHazards;
using RoR2;
using RoR2.CharacterAI;
using System;
using System.Linq;
using UnityEngine;

namespace PactOfPunishment.Waves.Halcyonites
{
    public partial class WhirlWindModule
    {
        public class UseAirNodesController : MonoBehaviour, IOverrideTargetPos, IOnOff
        {
#if DEBUG

            static UseAirNodesController()
            {
                EnableDebugVisuals = true;
            }

#endif

            private bool enableDebugVisuals;

            public static bool EnableDebugVisuals { get; set; }

            private UseAirNodes? instance;

            private LineRenderer lineRenderer;

            private EntityStateMachine weaponStateMachine;

            private GameObject debugSphere;

            Vector3 IOverrideTargetPos.TargetPos => this.Instance.currentTargetPosition;

            Vector3 IOverrideTargetPos.TargetMoveDirection => this.Instance.currentTargetMoveDirection;

            private EntityStateMachine.CommonComponentCache CommonComponents => this.weaponStateMachine.commonComponents;

            private UseAirNodes? Instance
            {
                get => this.instance;

                set
                {
                    this.instance = value;
                    this.UpdateDebugVisuals();
                }
            }

            public bool CustomEnabled { get; private set; }

            public void Activate()
            {
                if (this.CustomEnabled)
                {
                    return;
                }

                this.CustomEnabled = true;

                if (this.weaponStateMachine?.state is WhirlWindPersuitCycle whirlWindState)
                {
                    this.CreateInstance(whirlWindState);
                }
            }

            public void Deactivate()
            {
                if (!this.CustomEnabled)
                {
                    return;
                }

                this.CustomEnabled = false;
                this.Instance = null;
            }

            public void Update()
            {
                if (this.Instance is UseAirNodes instance)
                {
                    this.debugSphere.transform.position = instance.currentTargetPosition;
                }
            }

            public void OnEnable()
            {
                this.enableDebugVisuals = EnableDebugVisuals;
                this.UpdateDebugVisuals();
            }

            public void OnDisable()
            {
                this.enableDebugVisuals = false;
                this.UpdateDebugVisuals();
            }

            public void OnDestroy()
            {
                Destroy(this.debugSphere);
                Destroy(this.lineRenderer);
            }

            public Vector3 GetCurrentPosition(WhirlWindPersuitCycle state) => state.characterBody.corePosition;

            public void Awake()
            {
                this.lineRenderer = this.GetLineRenderer();
                this.lineRenderer.material = DebugOverlay.defaultWireMaterial;
                this.weaponStateMachine = EntityStateMachine.FindByCustomName(this.gameObject, "Weapon");
                this.debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                this.debugSphere.transform.localScale = Vector3.one * 0.5f;
            }

            private LineRenderer GetLineRenderer()
            {
                LineRenderer output;

                // output = this.gameObject.AddComponent<LineRenderer>(); output.useWorldSpace = true;

                output = Stage.instance.EnsureComponent<LineRenderer>();

                return output;
            }

            public void OnDashStart(WhirlWindPersuitCycle state) => this.CreateInstance(state);

            private void CreateInstance(WhirlWindPersuitCycle state) => this.Instance = new UseAirNodes(this.GetPath(state), state, this.GetCurrentPosition);

            void IOverrideTargetPos.Reset() => this.Instance = default;

            void IOverrideTargetPos.Update() => this.Instance?.Update();

            private void UpdateLineRenderer(Vector3[] positions)
            {
                if (this.lineRenderer != null)
                {
                    this.lineRenderer.positionCount = positions.Length;
                    this.lineRenderer.SetPositions(positions);
                }
            }

            private void UpdateDebugVisuals()
            {
                this.debugSphere?.SetActive(this.enableDebugVisuals);

                if (this.enableDebugVisuals)
                {
                    this.UpdateLineRenderer(this.Instance?.path?.Where(x => x.HasValue).Select(x => x!.Value).ToArray() ?? Array.Empty<Vector3>());
                }
                else
                {
                    this.UpdateLineRenderer(Array.Empty<Vector3>());
                }
            }

            private Path? GetPath(WhirlWindPersuitCycle state)
            {
                var path = new Path(SceneInfo.instance.airNodes);
                var pathTask = SceneInfo.instance.airNodes.ComputePath(new TryNotToNavigateThroughTheVoidFog.DangerAwarePathRequest
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

            public bool CheckIfArrived(bool orig)
            {
                if (this.Instance?.IsInFinalStretch == false)
                {
                    return false;
                }

                return orig;
            }

            private class UseAirNodes
            {
                public Vector3?[]? path;

                public int indexInPath;

                public Vector3 currentTargetPosition;

                public Vector3 currentTargetMoveDirection;

                private readonly WhirlWindPersuitCycle state;

                private readonly Func<Vector3> getCurrentPosition;

                public bool IsInFinalStretch => this.path == null || this.indexInPath >= this.path.Length - 1;

                public UseAirNodes(Path? path, WhirlWindPersuitCycle state, Func<WhirlWindPersuitCycle, Vector3> getCurrentPosition)
                {
                    this.path = path == null ? null : Enumerable.Range(0, path.waypointsCount).Select<int, Vector3?>(i =>
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
                    if (this.path == null)
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
                    if (this.path == null || this.path.Length == 0)
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
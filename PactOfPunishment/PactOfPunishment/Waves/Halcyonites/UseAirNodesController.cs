using EntityStates.Halcyonite;
using RoR2;
using RoR2.CharacterAI;
using System;
using System.Linq;
using UnityEngine;

namespace PactOfPunishment.Waves.Halcyonites
{
    public partial class WhirlWindModule
    {
        public class UseAirNodesController : MonoBehaviour, IOverrideTargetPos
        {
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

                    var positions = value?.path?.Where(x => x.HasValue).Select(x => x!.Value).ToArray() ?? Array.Empty<Vector3>();
                    this.lineRenderer.positionCount = positions.Length;
                    this.lineRenderer.SetPositions(positions);
                }
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
                this.lineRenderer = this.gameObject.AddComponent<LineRenderer>();
                this.lineRenderer.material = DebugOverlay.defaultWireMaterial;
                this.lineRenderer.useWorldSpace = true;
                this.weaponStateMachine = EntityStateMachine.FindByCustomName(this.gameObject, "Weapon");
                this.debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                this.debugSphere.transform.localScale = Vector3.one * 0.5f;
            }

            public void OnDashStart(WhirlWindPersuitCycle state) => this.Instance = new UseAirNodes(this.GetPath(state), state, this.GetCurrentPosition);

            void IOverrideTargetPos.Reset() => this.Instance = default;

            void IOverrideTargetPos.Update() => this.Instance?.Update();

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

            private class UseAirNodes
            {
                public Vector3?[]? path;

                public int indexInPath;

                public Vector3 currentTargetPosition;

                public Vector3 currentTargetMoveDirection;

                private readonly WhirlWindPersuitCycle state;

                private readonly Func<Vector3> getCurrentPosition;

                public UseAirNodes(Path? path, WhirlWindPersuitCycle state, Func<WhirlWindPersuitCycle, Vector3> getCurrentPosition)
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
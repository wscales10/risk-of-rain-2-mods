using HarmonyLib;
using HG;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PactOfPunishment.ProtectMonstersFromHazards
{
    public class TryNotToNavigateThroughTheVoidFog : Module
    {
        private bool hasLoggedWarning;

        private delegate void ConfigureAgentFromBody(ref NodeGraphNavigationSystem.AgentData reference, CharacterBody body);

        private delegate void ConfigureAgent(ref NodeGraphNavigationSystem.AgentData reference);

        public interface IFogAnalyzer
        {
            void ConfigureFromBody(CharacterBody? body);

            void ModifyScore(ref float score, Vector3 positionA, Vector3 positionB);
        }

        public override void Init()
        {
            IL.RoR2.Navigation.NodeGraph.ComputePath += Utils.HookIL(this.NodeGraph_ComputePath);
            IL.RoR2.Navigation.NodeGraphNavigationSystem.ConfigureAgentFromBody += Utils.HookIL(NodeGraphNavigationSystem_ConfigureAgentFromBody);
            On.RoR2.FogDamageController.Start += this.FogDamageController_Start;
            On.RoR2.Run.Awake += this.Run_Awake;
            IL.RoR2.Navigation.NodeGraphNavigationSystem.AgentData.Initialize += Utils.HookIL(AgentData_Initialize);
        }

        private static void AgentData_Initialize(ILCursor c)
        {
            c.GotoNext(x => x.MatchNewobj<NodeGraph.PathRequest>());
            c.Remove();
            c.Emit(OpCodes.Newobj, AccessTools.DeclaredConstructor(typeof(DangerAwarePathRequest)));
        }

        private static void NodeGraphNavigationSystem_ConfigureAgentFromBody(ILCursor c)
        {
            c.GotoNext(
                x => x.MatchLdloc(0),
                x => x.MatchLdarg(2),
                x => x.MatchLdfld<CharacterBody>(nameof(CharacterBody.hullClassification)),
                x => x.MatchStfld<NodeGraphNavigationSystem.AgentData>(nameof(NodeGraphNavigationSystem.AgentData.hullClassification))
            );

            c.Emit(OpCodes.Ldloc_0);
            c.Emit(OpCodes.Ldarg_2);
            c.EmitDelegate<ConfigureAgentFromBody>((ref NodeGraphNavigationSystem.AgentData reference, CharacterBody body) =>
            {
                if (reference.pathRequest is DangerAwarePathRequest dangerAwarePathRequest)
                {
                    dangerAwarePathRequest.ConfigureFromBody(body);
                }
            });

            c.GotoNext(MoveType.AfterLabel,
                x => x.MatchLdloc(0),
                x => x.MatchLdcI4(0),
                x => x.MatchStfld<NodeGraphNavigationSystem.AgentData>(nameof(NodeGraphNavigationSystem.AgentData.hullClassification))
            );
            c.Emit(OpCodes.Ldloc_0);
            c.EmitDelegate<ConfigureAgent>((ref NodeGraphNavigationSystem.AgentData reference) =>
            {
                if (reference.pathRequest is DangerAwarePathRequest dangerAwarePathRequest)
                {
                    dangerAwarePathRequest.ConfigureFromBody(null);
                }
            });
        }

        private void Run_Awake(On.RoR2.Run.orig_Awake orig, Run self)
        {
            orig(self);
            self.EnsureComponent<FogDamageControllersTracker>();
        }

        private void FogDamageController_Start(On.RoR2.FogDamageController.orig_Start orig, FogDamageController self)
        {
            orig(self);
            Run.instance?.GetComponent<FogDamageControllersTracker>()?.AddFogDamageController(self);
        }

        private void NodeGraph_ComputePath(ILCursor c)
        {
            void MatchNodeIndexComparison(out int nodeIndexVariableNumber, out ILLabel? skipReturnTrueLabel)
            {
                int tempNodeIndexVariableNumber = nodeIndexVariableNumber = -1;
                ILLabel? tempSkipReturnTrueLabel = skipReturnTrueLabel = null;

                int matchCount = 0;
                while (c.TryGotoNext(MoveType.After,
                    x => x.MatchLdloc(out tempNodeIndexVariableNumber),
                    x => x.MatchLdfld<NodeGraph.NodeIndex>(nameof(NodeGraph.NodeIndex.nodeIndex)),
                    x => x.MatchLdloc(out _),
                    x => x.MatchLdfld<NodeGraph.NodeIndex>(nameof(NodeGraph.NodeIndex.nodeIndex)),
                    x => x.MatchBneUn(out tempSkipReturnTrueLabel)))
                {
                    matchCount++;
                    nodeIndexVariableNumber = tempNodeIndexVariableNumber;
                    skipReturnTrueLabel = tempSkipReturnTrueLabel;
                }

                switch (matchCount)
                {
                    case 0:
                        throw new KeyNotFoundException("Could not find node index comparison in ComputePath");

                    case 1:
                        break;

                    default:
                        throw new InvalidOperationException($"Found multiple ({matchCount}) node index comparisons in ComputePath, cannot determine which one to use");
                }
            }

            MatchNodeIndexComparison(out int nodeIndexVariableNumber, out ILLabel? skipReturnTrueLabel);

            var returnTrueLabel = c.MarkLabel();
            c.Index--;
            c.Remove();
            c.Emit(OpCodes.Beq_S, returnTrueLabel);
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_1);
            c.EmitLdloc(nodeIndexVariableNumber);
            c.EmitDelegate<Func<NodeGraph, NodeGraph.PathRequest, NodeGraph.NodeIndex, bool>>((self, pathRequest, nodeIndex) =>
            {
                return pathRequest is DangerAwarePathRequest dangerAwarePathRequest && dangerAwarePathRequest.UseTerminationPredicate && dangerAwarePathRequest.TerminationPredicate != null
                    && dangerAwarePathRequest.TerminationPredicate(nodeIndex, self);
            });
            c.Emit(OpCodes.Brfalse_S, skipReturnTrueLabel);

            int linkVariableNumber = -1;
            c.GotoNext(
                x => x.MatchLdloc(out linkVariableNumber),
                x => x.MatchLdfld<NodeGraph.Link>(nameof(NodeGraph.Link.distanceScore)),
                x => x.MatchStloc(out _));
            c.Index++;
            c.Remove();
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate<Func<NodeGraph.Link, NodeGraph, NodeGraph.PathRequest, float>>(this.GetLinkScore);

            ILLabel? skipLabel = null;
            c.GotoPrev(MoveType.After,
                x => x.MatchLdloc(linkVariableNumber),
                x => x.MatchLdfld<NodeGraph.Link>(nameof(NodeGraph.Link.hullMask)),
                x => x.MatchLdloc(out _),
                x => x.MatchAnd(),
                x => x.MatchBrfalse(out skipLabel)
            );

            c.EmitLdloc(linkVariableNumber);
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate<Func<NodeGraph.Link, NodeGraph, NodeGraph.PathRequest, bool>>((link, graph, pathRequest) =>
            {
                if (pathRequest is DangerAwarePathRequest dangerAwarePathRequest)
                {
                    Vector3 startPos = graph.nodes[link.nodeIndexA.nodeIndex].position;
                    Vector3 endPos = graph.nodes[link.nodeIndexB.nodeIndex].position;
                    return (startPos.y - endPos.y) < dangerAwarePathRequest.MaximumFallDistance;
                }
                else
                {
                    return true;
                }
            });
            c.Emit(OpCodes.Brfalse_S, skipLabel);
        }

        private float GetLinkScore(NodeGraph.Link link, NodeGraph graph, NodeGraph.PathRequest pathRequest)
        {
            var score = link.distanceScore;

            if (graph.GetNodePosition(link.nodeIndexA, out var positionA) && graph.GetNodePosition(link.nodeIndexB, out var positionB))
            {
                if (pathRequest is DangerAwarePathRequest dangerAwarePathRequest && (dangerAwarePathRequest.FogAnalyzer != null))
                {
                    dangerAwarePathRequest.FogAnalyzer.ModifyScore(ref score, positionA, positionB);
                }
                else
                {
                    this.LogWarning();
                }
            }

            return score;
        }

        private void LogWarning()
        {
            if (!this.hasLoggedWarning)
            {
                this.Logger.LogError("path request is not fog-aware");
                this.hasLoggedWarning = true;
            }
        }

        public class FogDamageControllersTracker : MonoBehaviour
        {
            private readonly HashSet<FogDamageController> fogDamageControllers = new HashSet<FogDamageController>();

            private readonly List<FogDamageController> tempList = new List<FogDamageController>();

            public IEnumerable<FogDamageController> EnabledFogDamageControllers => this.fogDamageControllers.Where(controller => controller.enabled);

            public void AddFogDamageController(FogDamageController controller)
            {
                this.fogDamageControllers.Add(controller);
            }

            public void Update()
            {
                this.tempList.Clear();
                this.tempList.AddRange(this.fogDamageControllers);

                foreach (var controller in this.tempList.Where(controller => !controller))
                {
                    this.fogDamageControllers.Remove(controller);
                }
            }
        }

        public class DangerAwarePathRequest : NodeGraph.PathRequest
        {
            public bool UseTerminationPredicate { get; set; }

            public Func<NodeGraph.NodeIndex, NodeGraph, bool>? TerminationPredicate { get; set; }

            public IFogAnalyzer? FogAnalyzer { get; set; } = new FogAnalyzer();

            public float MaximumFallDistance { get; set; } = float.PositiveInfinity;

            public void ConfigureFromBody(CharacterBody? body)
            {
                if (body != null)
                {
                    this.MaximumFallDistance = Mathf.Pow(20f + body.baseJumpPower, 2) / 60f;
                    this.FogAnalyzer?.ConfigureFromBody(body);

                    this.TerminationPredicate = (nodeIndex, nodeGraph) =>
                    {
                        if (!nodeGraph)
                        {
                            return false;
                        }

                        nodeGraph.GetNodePosition(nodeIndex, out var pos);

                        // TODO: consider unifying one or more of this, the checks in FogAnalyzer code and the IsSafeLocation method
                        var run = Run.instance as InfiniteTowerRun;
                        var safeZone = run?.safeWardController?.safeZone;

                        return safeZone != null && safeZone.IsInBounds(pos);
                    };
                }
                else
                {
                    this.TerminationPredicate = null;
                    this.MaximumFallDistance = float.PositiveInfinity;
                    this.FogAnalyzer?.ConfigureFromBody(null);
                }
            }
        }

        public class FogAnalyzer : IFogAnalyzer
        {
            public TeamIndex BodyTeamIndex { get; set; } = TeamIndex.None;

            private float FogCoefficient { get; } = 3f;

            public void ConfigureFromBody(CharacterBody? body)
            {
                this.BodyTeamIndex = body?.teamComponent?.teamIndex ?? TeamIndex.None;
            }

            public void ModifyScore(ref float score, Vector3 positionA, Vector3 positionB)
            {
                foreach (var _ in Run.instance.GetComponent<FogDamageControllersTracker>().EnabledFogDamageControllers
                                    .Where(fogDamageController => this.MeetsTeamFilter(fogDamageController.teamFilter, fogDamageController.invertTeamFilter))
                                    .Where(fogDamageController =>
                                    {
                                        // TODO: also consider whether link moves towards or away safe zone centre?
                                        return IsPointInFog(fogDamageController, positionB);
                                    }))
                {
                    score *= this.FogCoefficient;
                }
            }

            private static bool IsPointInFog(FogDamageController fog, Vector3 point)
            {
                return !fog.safeZones.Any(safeZone => safeZone.IsInBounds(point));
            }

            private bool MeetsTeamFilter(TeamFilter filter, bool invert)
            {
                // TODO: if R2API allows custom team indices, add support for this
                if (this.BodyTeamIndex == TeamIndex.None || this.BodyTeamIndex == TeamIndex.Count)
                {
                    return false;
                }

                TeamIndex filterIndex = filter.teamIndex;

                if (invert)
                {
                    return this.BodyTeamIndex != filterIndex && this.BodyTeamIndex != TeamIndex.Neutral; // The exception for TeamIndex.Neutral is weird but it *is* in the code, so...
                }
                else
                {
                    if (filterIndex == TeamIndex.None)
                    {
                        return true;
                    }

                    return filterIndex == this.BodyTeamIndex;
                }
            }
        }
    }
}
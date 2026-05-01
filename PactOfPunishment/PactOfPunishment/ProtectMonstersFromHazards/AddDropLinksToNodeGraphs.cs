using RoR2;
using RoR2.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static RoR2.Navigation.NodeGraph;
using static RoR2.Navigation.NodeGraph.NodeFilters;

namespace PactOfPunishment.ProtectMonstersFromHazards
{
    public class AddDropLinksToNodeGraphs
    {
        private static float MaximumDistanceScoreScalar = 1f;

        public static void AddDropLinksToNodeGraph(NodeGraph nodeGraph)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                AddLinksToNodeGraph(nodeGraph, GetDropLinks(nodeGraph));
            }
            finally
            {
                stopwatch.Stop();
                Debug.Log($"Added drop links in {stopwatch.Elapsed.TotalSeconds} seconds.");
            }
        }

        private static void AddLinksToNodeGraph(NodeGraph nodeGraph, IEnumerable<Link> links)
        {
            var newLinks = links.ToList();

            // Group new links by source node
            var grouped = newLinks
                .GroupBy(l => l.nodeIndexA.nodeIndex)
                .ToDictionary(g => g.Key, g => g.ToArray());

            int oldLength = nodeGraph.links.Length;
            int additionalCount = newLinks.Count;

            var rebuilt = new Link[oldLength + additionalCount];

            int writeIndex = 0;

            for (int i = 0; i < nodeGraph.nodes.Length; i++)
            {
                ref var node = ref nodeGraph.nodes[i];
                ref var linkListIndex = ref node.linkListIndex;

                int oldStart = linkListIndex.index;
                int oldSize = (int)linkListIndex.size;

                // 1. Copy existing links for this node
                Array.Copy(nodeGraph.links, oldStart, rebuilt, writeIndex, oldSize);
                linkListIndex.index = writeIndex;

                writeIndex += oldSize;

                // 2. Insert new links for this node (if any)
                if (grouped.TryGetValue(i, out var extraLinks))
                {
                    Array.Copy(extraLinks, 0, rebuilt, writeIndex, extraLinks.Length);
                    writeIndex += extraLinks.Length;

                    linkListIndex.size += (uint)extraLinks.Length;
                }
            }

            nodeGraph.links = rebuilt;
            Debug.Log($"Added {newLinks.Count} links to node graph");
        }

        private static IEnumerable<Link> GetDropLinks(NodeGraph nodeGraph)
        {
            float maximumDistanceScore = nodeGraph.links.Max(x => x.distanceScore) * MaximumDistanceScoreScalar;
            Debug.Log($"Maximum allowed distance score: {maximumDistanceScore}");

            var candidateStartNodes = new Dictionary<NodeIndex, CandidateInfo>();

            for (int i = 0; i < nodeGraph.nodes.Length; i++)
            {
                var nodeIndex = new NodeIndex(i);
                if (AreOutgoingLinksAllOnOneSideOfNode(nodeGraph, nodeIndex, out var nodePosition, out var averageDirection))
                {
                    candidateStartNodes.Add(nodeIndex, new CandidateInfo { NodePosition = nodePosition, SearchDirection = -averageDirection });
                }
            }

            var candidateEndNodes = new Dictionary<NodeIndex, CandidateInfo>();

            for (int i = 0; i < nodeGraph.nodes.Length; i++)
            {
                var nodeIndex = new NodeIndex(i);
                if (AreIncomingLinksAllOnOneSideOfNode(nodeGraph, nodeIndex, out var nodePosition, out var averageDirection))
                {
                    candidateEndNodes.Add(nodeIndex, new CandidateInfo { NodePosition = nodePosition, SearchDirection = -averageDirection });
                }
            }

            var filter = new NodeInSubsetFilter(candidateEndNodes.Keys.Select(x => nodeGraph.nodes[x.nodeIndex].linkListIndex.index));
            var candidateEndNodeIndices = new List<NodeIndex>();

            foreach (var kvp in candidateStartNodes)
            {
                NodeIndex candidateStartNodeIndex = kvp.Key;
                Vector3 candidateStartPos = kvp.Value.NodePosition;
                FindCandidateEndNodes(candidateStartPos, maximumDistanceScore);

                foreach (var candidateEndNodeIndex in candidateEndNodeIndices)
                {
                    if (TryGetLink(candidateEndNodeIndex, out var link))
                    {
                        yield return link;
                        break;
                    }
                }

                bool TryGetLink(NodeIndex candidateEndNodeIndex, out Link link)
                {
                    if (candidateEndNodeIndex == NodeIndex.invalid)
                    {
                        link = default;
                        return false;
                    }

                    if (!candidateEndNodes.TryGetValue(candidateEndNodeIndex, out var endInfo))
                    {
                        link = default;
                        return false;
                    }

                    if (Vector3.Dot(kvp.Value.SearchDirection, endInfo.SearchDirection) > 0)
                    {
                        link = default;
                        return false;
                    }

                    Vector3 candidateEndPos = endInfo.NodePosition;
                    if (!(IsInFront(candidateStartPos, kvp.Value.SearchDirection, candidateEndPos) && IsInFront(candidateEndPos, endInfo.SearchDirection, candidateStartPos)))
                    {
                        link = default;
                        return false;
                    }

                    if (!TryGetHullMask(nodeGraph, candidateStartNodeIndex, candidateEndNodeIndex, out int hullMask))
                    {
                        link = default;
                        return false;
                    }

                    link = new Link
                    {
                        distanceScore = Vector3.Distance(candidateStartPos, candidateEndPos),
                        gateIndex = 0,
                        hullMask = hullMask,
                        jumpHullMask = 0,
                        maxSlope = 0,
                        minJumpHeight = 0,
                        nodeIndexA = candidateStartNodeIndex,
                        nodeIndexB = candidateEndNodeIndex
                    };
                    return true;
                }
            }

            void FindCandidateEndNodes(Vector3 position, float maxDistance = float.PositiveInfinity)
            {
                var nodeSearchFilter = Create(nodeGraph, And(filter, new NodeMinDistanceFilter(position, 1), new NodeDropHeightFilter(position)));
                candidateEndNodeIndices.Clear();
                nodeGraph.blockMap.GetNearestItemsWhichPassFilter(position, maxDistance, ref nodeSearchFilter, candidateEndNodeIndices);
            }
        }

        private static bool IsInFront(Vector3 aStart, Vector3 aDirection, Vector3 bStart)
        {
            return Vector3.Dot(aDirection, bStart - aStart) > 0f;
        }

        private static bool ArePointsAllOnOneSideOfNode(NodeGraph nodeGraph, NodeIndex node, Vector3[] points, out Vector3 startPoint, out Vector3 averageDirection)
        {
            startPoint = GetNodePosition(nodeGraph, node);

            int count = points.Length;

            if (count == 0)
            {
                averageDirection = Vector3.zero;
                return false;
            }

            if (count == 1)
            {
                Vector3 v = points[0] - startPoint;
                v.y = 0f;
                averageDirection = v.sqrMagnitude > 1e-8f ? v.normalized : Vector3.zero;
                return true;
            }

            float[] angles = new float[count];

            for (int i = 0; i < count; i++)
            {
                Vector3 v = points[i] - startPoint;
                angles[i] = Mathf.Atan2(v.z, v.x);
            }

            Array.Sort(angles);

            float maxGap = 0f;
            int maxGapIndex = -1;

            // Find largest gap
            for (int i = 0; i < count - 1; i++)
            {
                float gap = angles[i + 1] - angles[i];
                if (gap > maxGap)
                {
                    maxGap = gap;
                    maxGapIndex = i;
                }
            }

            // Wraparound gap
            float wrapGap = (angles[0] + Mathf.PI * 2f) - angles[count - 1];
            if (wrapGap > maxGap)
            {
                maxGap = wrapGap;
                maxGapIndex = count - 1;
            }

            bool isOneSide = maxGap >= Mathf.PI;

            if (!isOneSide)
            {
                averageDirection = Vector3.zero;
                return false;
            }

            // Compute midpoint of the complementary arc
            float startAngle = angles[(maxGapIndex + 1) % count];
            float arcSize = (Mathf.PI * 2f) - maxGap;

            float midAngle = startAngle + arcSize * 0.5f;

            // Normalize angle to (-π, π]
            if (midAngle > Mathf.PI)
                midAngle -= Mathf.PI * 2f;

            averageDirection = new Vector3(Mathf.Cos(midAngle), 0f, Mathf.Sin(midAngle));
            return true;
        }

        private static bool AreIncomingLinksAllOnOneSideOfNode(NodeGraph nodeGraph, NodeIndex node, out Vector3 nodePosition, out Vector3 averageDirection)
        {
            var links = nodeGraph.links.Where(link => link.nodeIndexB == node);
            Vector3[] endPoints = links.Select(link => GetNodePosition(nodeGraph, link.nodeIndexA)).ToArray();
            return ArePointsAllOnOneSideOfNode(nodeGraph, node, endPoints, out nodePosition, out averageDirection);
        }

        private static bool AreOutgoingLinksAllOnOneSideOfNode(NodeGraph nodeGraph, NodeIndex node, out Vector3 nodePosition, out Vector3 averageDirection)
        {
            var linkIndices = nodeGraph.GetActiveNodeLinks(node);
            Vector3[] endPoints = linkIndices.Select(linkIndex => GetNodePosition(nodeGraph, nodeGraph.links[linkIndex.linkIndex].nodeIndexB)).ToArray();
            return ArePointsAllOnOneSideOfNode(nodeGraph, node, endPoints, out nodePosition, out averageDirection);
        }

        private static Vector3 GetNodePosition(NodeGraph nodeGraph, NodeIndex nodeIndex)
        {
            if (!nodeGraph.GetNodePosition(nodeIndex, out var position))
            {
                throw new InvalidOperationException();
            }

            return position;
        }

        private static bool TryGetHullMask(NodeGraph nodeGraph, NodeIndex startNode, NodeIndex endNode, out int hullMask)
        {
            var startPos = GetNodePosition(nodeGraph, startNode);
            var endPos = GetNodePosition(nodeGraph, endNode);

            hullMask = 0;

            if (!NodeDropHeightFilter.IsAcceptableDropHeight(startPos.y - endPos.y))
            {
                return false;
            }

            for (int i = 0; i < HullDef.hullDefs.Length; i++)
            {
                var hullDef = HullDef.hullDefs[i];

                if (IsLinkUnobstructed(startPos, endPos, hullDef))
                {
                    hullMask |= (1 << i);
                }
            }

            return hullMask != 0;
        }

        private static bool IsLinkUnobstructed(Vector3 startPos, Vector3 endPos, HullDef hullDef)
        {
            // TODO: maybe go across a little bit, down then across rather than across a lot then down

            Vector3 pointNearFeetStart = startPos + 0.3f * Vector3.up;
            Vector3 pointNearWaistStart = startPos + 0.5f * hullDef.height * Vector3.up;

            if (IsObstructed(pointNearFeetStart, pointNearWaistStart))
            {
                return false;
            }

            Vector3 shiftedWaistPoint = new Vector3(endPos.x, pointNearWaistStart.y, endPos.z);

            if (IsObstructed(pointNearWaistStart, shiftedWaistPoint))
            {
                return false;
            }

            Vector3 pointNearFeetEnd = endPos + 0.3f * Vector3.up;
            if (IsObstructed(shiftedWaistPoint, pointNearFeetEnd))
            {
                return false;
            }

            return true;
        }

        private static bool IsObstructed(Vector3 startPos, Vector3 endPos)
        {
            var vector = endPos - startPos;
            return Physics.Raycast(startPos, vector, vector.magnitude, LayerIndex.world.mask, QueryTriggerInteraction.Ignore);
        }

        private struct CandidateInfo
        {
            public Vector3 NodePosition;

            public Vector3 SearchDirection;
        }

        private readonly struct NodeDropHeightFilter : INodeCheckFilterComponent
        {
            private readonly Vector3 dropStartPosition;

            public NodeDropHeightFilter(Vector3 position)
            {
                this.dropStartPosition = position;
            }

            public static bool IsAcceptableDropHeight(float dropHeight)
            {
                return dropHeight > 0;
            }

            public bool CheckNode(NodeGraph nodeGraph, ref Node node)
            {
                float dropHeight = this.dropStartPosition.y - node.position.y;
                return IsAcceptableDropHeight(dropHeight);
            }
        }

        private readonly struct NodeInSubsetFilter : INodeCheckFilterComponent
        {
            private readonly int[] listLinkIndexIndices;

            public NodeInSubsetFilter(IEnumerable<int> listLinkIndexIndices)
            {
                this.listLinkIndexIndices = listLinkIndexIndices.ToArray();
            }

            public bool CheckNode(NodeGraph nodeGraph, ref Node node)
            {
                return this.listLinkIndexIndices.Contains(node.linkListIndex.index);
            }
        }
    }
}
using RoR2.Navigation;

namespace PactOfPunishment.ProtectMonstersFromHazards
{
    using static NodeGraph;
    using static NodeGraph.NodeFilters;

    public readonly struct NodeSafeFilter : INodeCheckFilterComponent
    {
        public bool CheckNode(NodeGraph nodeGraph, ref Node node)
        {
            return Utils.IsSafeLocation(node.position);
        }
    }
}
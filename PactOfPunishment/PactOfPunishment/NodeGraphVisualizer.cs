using RoR2;
using RoR2.Navigation;
using System.Linq;
using UnityEngine;

namespace PactOfPunishment
{
    public class NodeGraphVisualizer : MonoBehaviour
    {
        private DebugOverlay.MeshDrawer? meshDrawer;

        private NodeGraph? nodeGraph;

        public NodeGraph? NodeGraph
        {
            get => this.nodeGraph;

            set
            {
                this.nodeGraph = value;
                this.TryDrawNodeGraph();
            }
        }

        public static void Add(NodeGraph nodeGraph)
        {
            Stage.instance.gameObject.AddComponent<NodeGraphVisualizer>().NodeGraph = nodeGraph;
        }

        public static void Clear()
        {
            foreach (var component in Stage.instance.gameObject.GetComponents<NodeGraphVisualizer>().ToArray())
            {
                Destroy(component);
            }
        }

        public void OnDestroy()
        {
            this.meshDrawer?.Dispose();
        }

        private void TryDrawNodeGraph()
        {
            this.meshDrawer?.Dispose();

            if (!this.NodeGraph)
            {
                return;
            }

            this.meshDrawer = new DebugOverlay.MeshDrawer(this.transform)
            {
                mesh = this.NodeGraph!.GenerateLinkDebugMesh(HullMask.Human)
            };
        }
    }
}
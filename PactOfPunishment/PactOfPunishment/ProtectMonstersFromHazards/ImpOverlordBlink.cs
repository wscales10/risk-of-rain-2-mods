using MonoMod.Cil;
using RoR2;
using RoR2.Navigation;
using System;
using UnityEngine;
using static RoR2.Navigation.NodeGraph;

namespace PactOfPunishment.ProtectMonstersFromHazards
{
    internal class ImpOverlordBlink : Module
    {
        public override void Init()
        {
            IL.EntityStates.ImpBossMonster.BlinkState.CalculateBlinkDestination += Utils.HookIL(BlinkState_CalculateBlinkDesination);
        }

        private static void BlinkState_CalculateBlinkDesination(ILCursor c)
        {
            c.GotoLast(x => x.MatchCallvirt<NodeGraph>(nameof(NodeGraph.FindClosestNode)));
            c.Remove();
            c.EmitDelegate<Func<NodeGraph, Vector3, HullClassification, float, NodeIndex>>((self, position, hullClassification, maxDistance) =>
            {
                var nodeIndex = self.FindClosestSafeNode(position, hullClassification, maxDistance);

                if (nodeIndex == NodeIndex.invalid)
                {
                    nodeIndex = self.FindClosestNode(position, hullClassification, maxDistance);
                }

                return nodeIndex;
            });
        }
    }
}
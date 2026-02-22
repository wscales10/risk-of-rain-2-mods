using EntityStates.ChildMonster;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PactOfPunishment
{
    public class ImproveChildMonsterAI : Module
    {
        public override void Init()
        {
            IL.EntityStates.ChildMonster.Frolic.TeleportAroundPlayer += Utils.HookIL(Frolic_TeleportAroundPlayer);
            On.EntityStates.ChildMonster.FrolicAway.TeleportAway += this.FrolicAway_TeleportAway; // TODO: use IL hooking instead?
        }

        private static void Frolic_TeleportAroundPlayer(ILCursor c)
        {
            int vectorVariableNumber = -1;
            c.GotoLast(x => x.MatchLdloc(out vectorVariableNumber), x => x.MatchCall(typeof(TeleportHelper), nameof(TeleportHelper.TeleportBody)));

            int flagVariableNumber = c.Body.Variables.Single(x => x.VariableType.MetadataType == MetadataType.Boolean).Index;
            ILLabel? setFlagToTrueLabel = null;
            c.GotoPrev(
                x => x.MatchLdcR4(35),
                x => x.MatchBgt(out setFlagToTrueLabel),
                x => x.MatchLdloc(out _),
                x => x.MatchLdcI4(0),
                x => x.MatchBge(out _),
                x => x.MatchLdcI4(1),
                x => x.MatchStloc(flagVariableNumber));
            c.Index++;
            c.Emit(OpCodes.Ldloc_S, (byte)vectorVariableNumber);
            c.EmitDelegate<Func<float, float, Vector3, bool>>((distance, minDistance, destination) => distance > minDistance && IsSafeTeleportLocation(destination));
            c.Remove();
            c.Emit(OpCodes.Brtrue_S, setFlagToTrueLabel);
        }

        private static bool IsSafeTeleportLocation(Vector3 position)
        {
            if (Run.instance is InfiniteTowerRun run) // TODO: apply to other fog damage controllers too? to lava?
            {
                return run.fogDamageController.safeZones.Any(x => x.IsInBounds(position));
            }

            return true;
        }

        private void FrolicAway_TeleportAway(On.EntityStates.ChildMonster.FrolicAway.orig_TeleportAway orig, FrolicAway self)
        {
            CharacterModel component = self.GetComponent<ModelLocator>().modelTransform.GetComponent<CharacterModel>();
            _ = self.characterBody.corePosition;
            NodeGraph nodeGraph = SceneInfo.instance.GetNodeGraph(MapNodeGroup.GraphType.Ground);

            Vector3 position = default;

            if (!(TryFindAcceptableDestination(100f, 200f) || TryFindAcceptableDestination(35f, 100f)))
            {
                this.Logger.LogWarning("Failed to find acceptable teleport destination");
            }

            bool TryFindAcceptableDestination(float minDistance, float maxDistance)
            {
                List<NodeGraph.NodeIndex> source = nodeGraph.FindNodesInRange(self.characterBody.corePosition, minDistance, maxDistance, HullMask.Human);

                for (int i = 0; i < source.Count; i++)
                {
                    nodeGraph.GetNodePosition(source[i], out position);

                    if (IsSafeTeleportLocation(position))
                    {
                        return true;
                    }
                }

                return false;
            }

            TeleportHelper.TeleportBody(self.characterBody, position, false);
            TeleportOutController.AddTPOutEffect(component, 1f, 0f, 1f);
            if (FrolicAway.tpEffectPrefab)
            {
                self.FireTPEffect();
            }
        }
    }
}
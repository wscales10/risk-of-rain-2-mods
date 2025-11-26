using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RoR2Mod1
{
    internal static class Utils
    {
        public static List<HoldoutZoneController> HoldoutZones { get; private set; }

        public static void SetHoldoutZones()
        {
            HoldoutZones = InstanceTracker.GetInstancesList<HoldoutZoneController>();
        }

        internal static bool IsInHoldoutZone(CharacterMaster master)
        {
            var bodyInstanceObject = master?.bodyInstanceObject;

            CharacterBody body;

            try
            {
                body = bodyInstanceObject?.GetComponent<CharacterBody>();
            }
            catch
            {
                return false;
            }

            if (body is null)
            {
                return false;
            }

            return HoldoutZones.Any(z => z.IsBodyInChargingRadius(body));
        }

        internal static bool IsInHoldoutZone(Vector3? position)
        {
            if (position is null)
            {
                return false;
            }

            return HoldoutZones.Any(z => z.IsInBounds((Vector3)position));
        }

        internal static bool WantsToStayInZone(CharacterMaster master)
        {
            if (master is null)
            {
                return false;
            }

            if (master.aiComponents.Any(ai => ai.isHealer))
            {
                return false;
            }

            SetHoldoutZones();

            if (!IsInHoldoutZone(master))
            {
                return false;
            }

            if (IsInHoldoutZone(GetMinionMaster(master)))
            {
                return false;
            }

            return true;
        }

        internal static CharacterMaster GetMinionMaster(CharacterMaster master)
        {
            return master ? master.minionOwnership.ownerMaster : null;
        }

        internal static string GetListOfAIsRunningBody(CharacterBody body)
        {
            var aiComponents = body?.master?.aiComponents;

            if (aiComponents is null)
            {
                return string.Empty;
            }

            return string.Join(", ", aiComponents.Select(x => x?.skillDriverEvaluation.dominantSkillDriver?.customName).Where(x => x != null));
        }

        internal static int GetTotalNumberOfPlayers(TeamIndex teamIndex)
        {
            return TeamComponent.GetTeamMembers(teamIndex).Count(teamComponent => teamComponent.body?.isPlayerControlled ?? false);
        }

        internal static HashSet<CharacterBody> GetBodiesInHoldoutZone(HoldoutZoneController holdoutZoneController, TeamIndex teamIndex)
        {
            return TeamComponent.GetTeamMembers(teamIndex).Select(tc => tc.body).Where(body => holdoutZoneController.IsBodyInChargingRadius(body)).ToHashSet();
        }
    }
}
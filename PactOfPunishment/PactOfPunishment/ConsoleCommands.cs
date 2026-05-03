using BepInEx;
using PactOfPunishment.Waves.Infrastructure;
using RoR2;
using RoR2.Navigation;
using System.Linq;
using UnityEngine;

namespace PactOfPunishment
{
    public partial class PactOfPunishmentPlugin : BaseUnityPlugin
    {
        [ConCommand(commandName = "simulacrum_complete_wave", flags = ConVarFlags.ExecuteOnServer, helpText = "Completes the current simulacrum wave.")]
        private static void CmdCompleteSimulacrumWave(ConCommandArgs args)
        {
            CompleteCurrentSimulacrumWave();
        }

        private static void CompleteCurrentSimulacrumWave()
        {
            if (Run.instance is InfiniteTowerRun run && run.waveController is InfiniteTowerWaveController wave)
            {
                wave.Network_totalWaveCredits = wave.combatDirector.totalCreditsSpent;

                var teamMembers = TeamComponent.GetTeamMembers(TeamIndex.Monster);
                for (int k = teamMembers.Count - 1; k >= 0; k--)
                {
                    teamMembers[k].body.master?.TrueKill(wave.gameObject, wave.gameObject, DamageType.VoidDeath);
                }
            }
        }

        [ConCommand(commandName = "simulacrum_advance", flags = ConVarFlags.ExecuteOnServer, helpText = "Completes or starts a simulacrum wave.")]
        private static void CmdSimulacrumAdvance(ConCommandArgs args)
        {
            var safeWardState = Utils.GetSafeWardState();
            switch (safeWardState?.purchaseInteraction?.available)
            {
                case true:
                    safeWardState.safeWardController?.Activate();
                    break;

                case false:
                    CompleteCurrentSimulacrumWave();
                    break;
            }
        }

        [ConCommand(commandName = "simulacrum_override_wave", flags = ConVarFlags.ExecuteOnServer, helpText = "Overrides the next simulacrum wave.")]
        private static void CmdOverrideNextSimulacrumWave(ConCommandArgs args)
        {
            if (Run.instance is InfiniteTowerRun run && run.GetComponent<SimulacrumWavesBehavior>() is SimulacrumWavesBehavior behavior && args.Count > 0)
            {
                behavior.WaveOverrideName = args[0];
            }
        }

        [ConCommand(commandName = "simulacrum_override_wave_index", flags = ConVarFlags.ExecuteOnServer, helpText = "Overrides the wave index used to choose the next simulacrum wave (-1 = no override).")]
        private static void CmdOverrideSimulacrumWaveIndex(ConCommandArgs args)
        {
            if (Run.instance is InfiniteTowerRun run && run.GetComponent<SimulacrumWavesBehavior>() is SimulacrumWavesBehavior behavior && args.Count > 0)
            {
                behavior.WaveOverrideIndex = args.GetArgInt(0);
            }
        }

        [ConCommand(commandName = "log_state_changes", helpText = "Toggles whether state changes are logged.")]
        private static void CmdToggleStateLogging(ConCommandArgs args)
        {
            Logging.Instance.LogStateChanges = args.GetArgBool(0);
        }

        [ConCommand(commandName = "summon_safe_ward", helpText = "Respawns the safe ward near the player.", flags = ConVarFlags.ExecuteOnServer)]
        private static void CmdSummonSafeWard(ConCommandArgs args)
        {
            if (!(Run.instance is InfiniteTowerRun run) || !args.senderBody || !args.senderBody.transform || !run.safeWardController)
            {
                return;
            }

            var nodeGraph = SceneInfo.instance.groundNodes;
            var nodeIndex = nodeGraph.FindClosestNode(args.senderBody.footPosition, run.safeWardCard.hullSize);

            if (nodeGraph.GetNodePosition(nodeIndex, out var position))
            {
                run.safeWardController.transform.position = position;

                var list = SpawnPoint.readOnlyInstancesList.ToList();

                for (int i = 0; i < list.Count; i++)
                {
                    Destroy(list[i].gameObject);
                }

                foreach (NodeGraph.NodeIndex spawnNode in nodeGraph.FindNodesInRangeWithFlagConditions(position, 0f, run.spawnMaxRadius, HullMask.Human, NodeFlags.None, NodeFlags.NoCharacterSpawn, preventOverhead: false))
                {
                    if (nodeGraph.GetNodePosition(spawnNode, out var spawnPointPosition))
                    {
                        SpawnPoint.AddSpawnPoint(spawnPointPosition, Quaternion.LookRotation(position, Vector3.up));
                    }
                }
            }
        }
    }
}
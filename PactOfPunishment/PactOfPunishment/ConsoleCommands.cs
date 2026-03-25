using BepInEx;
using PactOfPunishment.Waves.Infrastructure;
using RoR2;

namespace PactOfPunishment
{
    public partial class PactOfPunishmentPlugin : BaseUnityPlugin
    {
        [ConCommand(commandName = "simulacrum_complete_wave", flags = ConVarFlags.ExecuteOnServer, helpText = "Completes the current simulacrum wave.")]
        private static void CmdCompleteSimulacrumWave(ConCommandArgs args)
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

        [ConCommand(commandName = "simulacrum_override_wave", flags = ConVarFlags.ExecuteOnServer, helpText = "Overrides the next simulacrum wave.")]
        private static void CmdOverrideNextSimulacrumWave(ConCommandArgs args)
        {
            if (Run.instance is InfiniteTowerRun run && run.GetComponent<SimulacrumWavesBehavior>() is SimulacrumWavesBehavior behavior && args.Count > 0)
            {
                behavior.WaveOverrideName = args[0];
            }
        }

        [ConCommand(commandName = "log_state_changes", helpText = "Toggles whether state changes are logged.")]
        private static void CmdToggleStateLogging(ConCommandArgs args)
        {
            Logging.Instance.LogStateChanges = args.GetArgBool(0);
        }
    }
}
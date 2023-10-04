using RoR2;

namespace Ror2Mod2
{
    internal partial class ContextHelper
    {
        private void CreditsController_OnEnable(On.RoR2.CreditsController.orig_OnEnable orig, CreditsController self)
        {
            orig(self);
            scenePart++;
            UpdateContext();
        }

        private void CombatSquad_AddMember(On.RoR2.CombatSquad.orig_AddMember orig, CombatSquad self, CharacterMaster memberMaster)
        {
            orig(self, memberMaster);
            UpdateContext();
        }

        private void Run_BeginGameOver(On.RoR2.Run.orig_BeginGameOver orig, Run self, GameEndingDef gameEndingDef)
        {
            orig(self, gameEndingDef);
            runOutcome = ConvertEndingToOutcome(gameEndingDef);
            UpdateContext();
        }

        private void PreEncounter_OnEnter(On.EntityStates.Missions.BrotherEncounter.PreEncounter.orig_OnEnter orig, EntityStates.Missions.BrotherEncounter.PreEncounter self)
        {
            orig(self);
            scenePart = 1;
            UpdateContext();
        }

        private void Phase1_OnEnter(On.EntityStates.Missions.BrotherEncounter.Phase1.orig_OnEnter orig, EntityStates.Missions.BrotherEncounter.Phase1 self)
        {
            orig(self);
            isBossEncounter = true;
            UpdateContext();
        }

        /// <summary>
        /// Increment <see cref="scenePart"/> when
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="self"></param>
        private void BrotherEncounterPhaseBaseState_OnExit(On.EntityStates.Missions.BrotherEncounter.BrotherEncounterPhaseBaseState.orig_OnExit orig, EntityStates.Missions.BrotherEncounter.BrotherEncounterPhaseBaseState self)
        {
            orig(self);
            scenePart++;
            UpdateContext();
        }

        private void EncounterFinished_OnEnter(On.EntityStates.Missions.BrotherEncounter.EncounterFinished.orig_OnEnter orig, EntityStates.Missions.BrotherEncounter.EncounterFinished self)
        {
            orig(self);
            isBossEncounter = false;
            UpdateContext();
        }

        private void CombatState_OnEnter(On.RoR2.ArtifactTrialMissionController.CombatState.orig_OnEnter orig, EntityStates.EntityState self)
        {
            orig(self);
            isBossEncounter = true;
            UpdateContext();
        }

        private void WaitForRewardTaken_OnEnter(On.RoR2.ArtifactTrialMissionController.WaitForRewardTaken.orig_OnEnter orig, EntityStates.EntityState self)
        {
            orig(self);
            isBossEncounter = false;
            UpdateContext();
        }

        private void GoldshoresBossfight_OnEnter(On.EntityStates.Missions.Goldshores.GoldshoresBossfight.orig_OnEnter orig, EntityStates.Missions.Goldshores.GoldshoresBossfight self)
        {
            orig(self);
            isBossEncounter = true;
            UpdateContext();
        }

        private void GoldshoresBossfight_OnExit(On.EntityStates.Missions.Goldshores.GoldshoresBossfight.orig_OnExit orig, EntityStates.Missions.Goldshores.GoldshoresBossfight self)
        {
            orig(self);
            isBossEncounter = false;
            UpdateContext();
        }

        private void VoidRaidEncounterController_Start(On.RoR2.VoidRaidEncounterController.orig_Start orig, VoidRaidEncounterController self)
        {
            orig(self);
            isBossEncounter = true;
            UpdateContext();
        }

        private void DeathState_OnExit(On.EntityStates.VoidRaidCrab.DeathState.orig_OnExit orig, EntityStates.VoidRaidCrab.DeathState self)
        {
            orig(self);
            isBossEncounter = false;
            UpdateContext();
        }

        private void SceneCatalog_onMostRecentSceneDefChanged(SceneDef sceneDef)
        {
            UpdateContext();
        }

        private void ChargedState_OnEnter(On.RoR2.TeleporterInteraction.ChargedState.orig_OnEnter orig, EntityStates.BaseState self)
        {
            orig(self);
            SetIsChargingTeleporter(false, suppressUpdate: true);
            UpdateContext();
        }

        private void IdleToChargingState_OnEnter(On.RoR2.TeleporterInteraction.IdleToChargingState.orig_OnEnter orig, EntityStates.BaseState self)
        {
            orig(self);
            UpdateContext();
        }

        private void ChargingState_OnEnter(On.RoR2.TeleporterInteraction.ChargingState.orig_OnEnter orig, EntityStates.BaseState self)
        {
            orig(self);
            SetIsChargingTeleporter(suppressUpdate: true);
            UpdateContext();
        }

        private void PlayerCharacterMasterController_OnEnable(On.RoR2.PlayerCharacterMasterController.orig_OnEnable orig, PlayerCharacterMasterController self)
        {
            orig(self);
            SetPlayerCharacterMaster();
        }

        private void PlayerCharacterMasterController_OnDisable(On.RoR2.PlayerCharacterMasterController.orig_OnDisable orig, PlayerCharacterMasterController self)
        {
            orig(self);
            SetPlayerCharacterMaster();
        }

        private void ChargingState_FixedUpdate(On.RoR2.TeleporterInteraction.ChargingState.orig_FixedUpdate orig, EntityStates.BaseState self)
        {
            SetIsChargingTeleporter();
        }
    }
}
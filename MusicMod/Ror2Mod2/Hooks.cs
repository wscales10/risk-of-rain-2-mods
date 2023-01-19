using RoR2;

namespace Ror2Mod2
{
	internal partial class ContextHelper
	{
		private void Phase1_OnEnter(On.EntityStates.Missions.BrotherEncounter.Phase1.orig_OnEnter orig, EntityStates.Missions.BrotherEncounter.Phase1 self)
		{
			orig(self);
			isBossEncounter = true;
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
			UpdateContext();
		}
	}
}
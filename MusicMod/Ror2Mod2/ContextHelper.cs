using MyRoR2;
using RoR2;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using Utils;
using static MyRoR2.SceneNameHelpers;

namespace Ror2Mod2
{
	internal partial class ContextHelper
	{
		private const bool WaitForBosses = false;
		private int scenePart;
		private bool isBossEncounter;
		private RunType runType;
		private MyScene oldScene;
		private Scene? currentScene;
		private readonly Logger Log;

		public ContextHelper(Func<Task> update, Logger logger)
		{
			On.RoR2.CreditsController.OnEnable += CreditsController_OnEnable;

			SceneManager.sceneUnloaded += (_) => currentScene = null;
			SceneManager.activeSceneChanged += OnSceneChanged;
			UpdateMusic = () => update();
			Log = logger;

			SceneCatalog.onMostRecentSceneDefChanged += SceneCatalog_onMostRecentSceneDefChanged;

			if (WaitForBosses)
			{
				On.RoR2.TeleporterInteraction.ChargingState.OnEnter += ChargingState_OnEnter;
			}
			else
			{
				On.RoR2.TeleporterInteraction.IdleToChargingState.OnEnter += IdleToChargingState_OnEnter;
			}

			On.RoR2.TeleporterInteraction.ChargedState.OnEnter += ChargedState_OnEnter;

			UnityEngine.Application.quitting += UpdateMusic;

			On.EntityStates.Missions.BrotherEncounter.Phase1.OnEnter += Phase1_OnEnter;
			On.EntityStates.Missions.BrotherEncounter.BrotherEncounterPhaseBaseState.OnExit += BrotherEncounterPhaseBaseState_OnExit;
			On.EntityStates.Missions.BrotherEncounter.EncounterFinished.OnEnter += EncounterFinished_OnEnter;

			On.RoR2.ArtifactTrialMissionController.CombatState.OnEnter += CombatState_OnEnter;
			On.RoR2.ArtifactTrialMissionController.WaitForRewardTaken.OnEnter += WaitForRewardTaken_OnEnter;

			On.EntityStates.Missions.Goldshores.GoldshoresBossfight.OnEnter += GoldshoresBossfight_OnEnter;
			On.EntityStates.Missions.Goldshores.GoldshoresBossfight.OnExit += GoldshoresBossfight_OnExit;

			On.RoR2.VoidRaidEncounterController.Start += VoidRaidEncounterController_Start;
			On.EntityStates.VoidRaidCrab.DeathState.OnExit += DeathState_OnExit;
		}

		private void BrotherEncounterPhaseBaseState_OnExit(On.EntityStates.Missions.BrotherEncounter.BrotherEncounterPhaseBaseState.orig_OnExit orig, EntityStates.Missions.BrotherEncounter.BrotherEncounterPhaseBaseState self)
		{
			orig(self);
			scenePart++;
			UpdateMusic();
		}

		private MyScene SceneName => new MyScene(currentScene?.name?.ToUpper());

		public Action UpdateMusic { get; }

		private void CreditsController_OnEnable(On.RoR2.CreditsController.orig_OnEnable orig, CreditsController self)
		{
			orig(self);
			scenePart++;
			UpdateMusic();
		}

		private void OnSceneChanged(Scene _, Scene newScene)
		{
			scenePart = 0;
			currentScene = newScene;
			Log($"set currentScene to {SceneName}");
			GetValueFromSceneName(out runType, SceneName,
				F(RunType.Simulacrum, Scenes.SimulacrumMenu),
				F(RunType.Eclipse, Scenes.EclipseMenu),
				F(RunType.Normal, () => ScenePattern.Equals(Scenes.MainMenu).IsMatch(oldScene), Scenes.CharacterSelect),
				F(RunType.None, Scenes.MainMenu)
				);
			Log(oldScene);
			Log(runType);
		}

		public Context GetContext()
		{
			Log("returning new Context");
			var bossGroup = TeleporterInteraction.instance?.GetComponent<BossGroup>() ?? InstanceTracker.GetInstancesList<BossGroup>().SingleOrDefault();
			return new Context()
			{
				SceneName = oldScene = SceneName,
				SceneType = SceneCatalog.mostRecentSceneDef?.sceneType ?? SceneType.Invalid,
				StageNumber = Run.instance?.stageClearCount + 1,
				WaveNumber = (Run.instance as InfiniteTowerRun)?.waveIndex,
				LoopIndex = Run.instance?.loopClearCount,
				BossBodyName = bossGroup?.bestObservedName,
				TeleporterState = TeleporterInteraction.instance?.activationState,
				IsBossEncounter = isBossEncounter || !(bossGroup is null),
				ScenePart = scenePart,
				RunType = GetRunType(Run.instance) ?? runType
			};
		}

		public RunType? GetRunType(Run run)
		{
			switch (run)
			{
				case null:
					return null;
				case EclipseRun _:
					return RunType.Eclipse;
				case InfiniteTowerRun _:
					return RunType.Simulacrum;
				case WeeklyRun _:
					return RunType.PrismaticTrial;
				default:
					return RunType.Normal;
			}
		}
	}
}

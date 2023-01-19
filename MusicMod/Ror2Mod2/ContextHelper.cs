using MyRoR2;
using RoR2;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;
using static MyRoR2.SceneNameHelpers;
using static RoR2.RoR2Content.GameEndings;
using static RoR2.DLC1Content.GameEndings;
using Logger = Utils.Logger;

namespace Ror2Mod2
{
	internal partial class ContextHelper : IContextHelper<Context>
	{
		private static readonly Regex bodyPrefabRegex = new Regex("BODY$");

		private readonly Logger Log;

		private int scenePart;

		private bool isBossEncounter;

		private RunType runType;

		private MyScene oldScene;

		private Scene? currentScene;

		private RunOutcome? runOutcome;

		public ContextHelper(Logger logger)
		{
			On.RoR2.CreditsController.OnEnable += CreditsController_OnEnable;

			SceneManager.sceneUnloaded += (_) =>
			{
				oldScene = SceneName;
				currentScene = null;
			};
			SceneManager.activeSceneChanged += OnSceneChanged;

			Log = logger;

			SceneCatalog.onMostRecentSceneDefChanged += SceneCatalog_onMostRecentSceneDefChanged;

			On.RoR2.TeleporterInteraction.ChargingState.OnEnter += ChargingState_OnEnter;

			On.RoR2.TeleporterInteraction.IdleToChargingState.OnEnter += IdleToChargingState_OnEnter;

			On.RoR2.TeleporterInteraction.ChargedState.OnEnter += ChargedState_OnEnter;

			Application.quitting += () => NewContext?.Invoke(default);

			On.EntityStates.Missions.BrotherEncounter.Phase1.OnEnter += Phase1_OnEnter;
			On.EntityStates.Missions.BrotherEncounter.BrotherEncounterPhaseBaseState.OnExit += BrotherEncounterPhaseBaseState_OnExit;
			On.EntityStates.Missions.BrotherEncounter.EncounterFinished.OnEnter += EncounterFinished_OnEnter;

			On.RoR2.ArtifactTrialMissionController.CombatState.OnEnter += CombatState_OnEnter;
			On.RoR2.ArtifactTrialMissionController.WaitForRewardTaken.OnEnter += WaitForRewardTaken_OnEnter;

			On.EntityStates.Missions.Goldshores.GoldshoresBossfight.OnEnter += GoldshoresBossfight_OnEnter;
			On.EntityStates.Missions.Goldshores.GoldshoresBossfight.OnExit += GoldshoresBossfight_OnExit;

			On.RoR2.VoidRaidEncounterController.Start += VoidRaidEncounterController_Start;
			On.EntityStates.VoidRaidCrab.DeathState.OnExit += DeathState_OnExit;

			On.RoR2.CombatSquad.AddMember += CombatSquad_AddMember;

			Run.onRunStartGlobal += Run_onRunStartGlobal;
			On.RoR2.Run.BeginGameOver += Run_BeginGameOver;
			Run.onRunDestroyGlobal += Run_onRunDestroyGlobal;
		}

		public event Action<Context> NewContext;

		private MyScene SceneName => new MyScene(currentScene?.name?.ToUpper());

		public void UpdateContext() => NewContext?.Invoke(GetContext());

		public Context GetContext()
		{
			Log("returning new Context");
			var bossGroup = TeleporterInteraction.instance?.GetComponent<BossGroup>() ?? InstanceTracker.GetInstancesList<BossGroup>().SingleOrDefault();
			var playerBodyPrefab = PlayerCharacterMasterController.instances.FirstOrDefault()?.master?.bodyPrefab;

			return new Context()
			{
				SceneName = SceneName,
				SceneType = SceneCatalog.mostRecentSceneDef?.sceneType ?? SceneType.Invalid,
				StageNumber = Run.instance?.stageClearCount + 1,
				WaveNumber = (Run.instance as InfiniteTowerRun)?.waveIndex,
				LoopIndex = Run.instance?.loopClearCount,
				BossBodyName = new Entity(bossGroup?.bestObservedName?.ToUpper()),
				Bosses = (bossGroup?.combatSquad?.readOnlyMembersList?.Select(m => GetEntity(m?.bodyPrefab)) ?? Enumerable.Empty<Entity>()).ToReadOnlyCollection(),
				TeleporterState = TeleporterInteraction.instance?.activationState,
				IsBossEncounter = isBossEncounter || !(bossGroup is null),
				ScenePart = scenePart,
				RunType = GetRunType(Run.instance) ?? runType,
				Survivor = GetEntity(playerBodyPrefab),
				Outcome = runOutcome
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

		private void Run_onRunDestroyGlobal(Run obj)
		{
			runOutcome = null;
			UpdateContext();
		}

		private void Run_BeginGameOver(On.RoR2.Run.orig_BeginGameOver orig, Run self, GameEndingDef gameEndingDef)
		{
			orig(self, gameEndingDef);
			runOutcome = ConvertEndingToOutcome(gameEndingDef);
			UpdateContext();
		}

		private RunOutcome ConvertEndingToOutcome(GameEndingDef ending)
		{
			if (ending.IsOneOf(LimboEnding, ObliterationEnding, VoidEnding))
			{
				return RunOutcome.FateUnknown;
			}
			else if (ending.IsOneOf(MainEnding, PrismaticTrialEnding))
			{
				return RunOutcome.Victory;
			}
			else if (ending.IsOneOf(StandardLoss))
			{
				return RunOutcome.Defeat;
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		private void Run_onRunStartGlobal(Run obj)
		{
			runOutcome = RunOutcome.Undecided;
			UpdateContext();
		}

		private void CombatSquad_AddMember(On.RoR2.CombatSquad.orig_AddMember orig, CombatSquad self, CharacterMaster memberMaster)
		{
			orig(self, memberMaster);
			UpdateContext();
		}

		private void BossGroup_UpdateBossMemories(On.RoR2.BossGroup.orig_UpdateBossMemories orig, BossGroup self)
		{
			orig(self);
			UpdateContext();
		}

		private void BossGroup_RememberBoss(On.RoR2.BossGroup.orig_RememberBoss orig, BossGroup self, CharacterMaster master)
		{
			orig(self, master);
			UpdateContext();
		}

		private Entity GetEntity(GameObject bodyPrefab)
		{
			var bodyPrefabString = bodyPrefab?.name?.ToUpper();
			return bodyPrefabString is null ? null : new Entity(bodyPrefabRegex.Replace(bodyPrefabString, string.Empty));
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

		private void CreditsController_OnEnable(On.RoR2.CreditsController.orig_OnEnable orig, CreditsController self)
		{
			orig(self);
			scenePart++;
			UpdateContext();
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
			Log($"{nameof(oldScene)}: {oldScene?.Name}");
			Log($"{nameof(runType)}: {runType}");
		}
	}
}
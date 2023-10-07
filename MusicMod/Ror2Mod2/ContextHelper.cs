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
    internal partial class ContextHelper : IContextHelper<RoR2Context>
    {
        private static readonly Regex bodyPrefabRegex = new Regex("BODY$");

        private readonly Logger Log;

        private CharacterMaster playerCharacterMaster;

        private int scenePart;

        private bool isBossEncounter;

        private RunType runType;

        private MyScene oldScene;

        private Scene? currentScene;

        private RunOutcome? runOutcome;

        private bool isChargingTeleporter;

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

            On.RoR2.TeleporterInteraction.ChargingState.FixedUpdate += ChargingState_FixedUpdate;

            Application.quitting += () => NewContext?.Invoke(default);

            On.EntityStates.Missions.BrotherEncounter.PreEncounter.OnEnter += PreEncounter_OnEnter;

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

            On.RoR2.PlayerCharacterMasterController.OnEnable += PlayerCharacterMasterController_OnEnable;
            On.RoR2.PlayerCharacterMasterController.OnDisable += PlayerCharacterMasterController_OnDisable;
        }

        public event Action<RoR2Context> NewContext;

        private MyScene SceneName => new MyScene(currentScene?.name?.ToUpper());

        public void UpdateContext() => NewContext?.Invoke(GetContext());

        public RoR2Context GetContext()
        {
            Log("returning new Context");
            var bossGroup = TeleporterInteraction.instance?.GetComponent<BossGroup>() ?? InstanceTracker.GetInstancesList<BossGroup>().SingleOrDefault();
            var playerBodyPrefab = playerCharacterMaster?.bodyPrefab;

            return new RoR2Context()
            {
                SceneName = SceneName,
                SceneType = SceneCatalog.mostRecentSceneDef?.sceneType.AsEnum<MyRoR2.SceneType>() ?? MyRoR2.SceneType.Invalid,
                StageNumber = Run.instance?.stageClearCount + 1,
                WaveNumber = (Run.instance as InfiniteTowerRun)?.waveIndex,
                LoopIndex = Run.instance?.loopClearCount,
                BossBodyName = new Entity(bossGroup?.bestObservedName?.ToUpper()),
                Bosses = (bossGroup?.combatSquad?.readOnlyMembersList?.Select(m => GetEntity(m?.bodyPrefab)) ?? Enumerable.Empty<Entity>()).ToReadOnlyCollection(),
                TeleporterState = TeleporterInteraction.instance?.activationState.AsEnum<TeleporterState>(),
                IsChargingTeleporter = isChargingTeleporter,
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

        private void SetIsChargingTeleporter(bool? input = null, bool suppressUpdate = false)
        {
            bool value = input ?? TeleporterInteraction.instance.holdoutZoneController.IsBodyInChargingRadius(playerCharacterMaster.GetBody());

            if (isChargingTeleporter == value)
            {
                return;
            }

            isChargingTeleporter = value;

            if (!suppressUpdate)
            {
                UpdateContext();
            }
        }

        private void Run_onRunDestroyGlobal(Run obj)
        {
            runOutcome = null;
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

        private void SetPlayerCharacterMaster()
        {
            // TODO: check in mutiplayer
            playerCharacterMaster = PlayerCharacterMasterController.instances.FirstOrDefault()?.master;
        }

        private Entity GetEntity(GameObject bodyPrefab)
        {
            var bodyPrefabString = bodyPrefab?.name?.ToUpper();
            return bodyPrefabString is null ? null : new Entity(bodyPrefabRegex.Replace(bodyPrefabString, string.Empty));
        }

        private void OnSceneChanged(Scene _, Scene newScene)
        {
            scenePart = 0;
            currentScene = newScene;
            Log($"set currentScene to {SceneName}");

            if (GetValueFromSceneName(out var newRunType, SceneName,
                F(RunType.Simulacrum, Scenes.SimulacrumMenu),
                F(RunType.Eclipse, Scenes.EclipseMenu),
                F(RunType.Normal, () => ScenePattern.Equals(Scenes.MainMenu).IsMatch(oldScene), Scenes.CharacterSelect),
                F(RunType.None, Scenes.MainMenu)
                ))
            {
                this.runType = newRunType;
            }

            Log($"{nameof(oldScene)}: {oldScene?.Name}");
            Log($"{nameof(newRunType)}: {newRunType}");
        }
    }
}
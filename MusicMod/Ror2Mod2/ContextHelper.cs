﻿using MyRoR2;
using RoR2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;
using static MyRoR2.SceneNameHelpers;
using Logger = Utils.Logger;

namespace Ror2Mod2
{
    internal partial class ContextHelper
    {
        private const bool WaitForBosses = false;

        private static readonly Regex bodyPrefabRegex = new Regex("BODY$");

        private readonly Logger Log;

        private int scenePart;

        private bool isBossEncounter;

        private RunType runType;

        private MyScene oldScene;

        private Scene? currentScene;

        public ContextHelper(Func<Task> update, Logger logger)
        {
            On.RoR2.CreditsController.OnEnable += CreditsController_OnEnable;

            SceneManager.sceneUnloaded += (_) =>
            {
                oldScene = SceneName;
                currentScene = null;
            };
            SceneManager.activeSceneChanged += OnSceneChanged;
            UpdateMusic = () => update();
            Log = logger;

            SceneCatalog.onMostRecentSceneDefChanged += SceneCatalog_onMostRecentSceneDefChanged;

            On.RoR2.TeleporterInteraction.ChargingState.OnEnter += ChargingState_OnEnter;

            On.RoR2.TeleporterInteraction.IdleToChargingState.OnEnter += IdleToChargingState_OnEnter;

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

            On.RoR2.CombatSquad.AddMember += CombatSquad_AddMember;
        }

        public Action UpdateMusic { get; }

        private MyScene SceneName => new MyScene(currentScene?.name?.ToUpper());

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
                Survivor = GetEntity(playerBodyPrefab)
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

        private void CombatSquad_AddMember(On.RoR2.CombatSquad.orig_AddMember orig, CombatSquad self, CharacterMaster memberMaster)
        {
            orig(self, memberMaster);
            UpdateMusic();
        }

        private void BossGroup_UpdateBossMemories(On.RoR2.BossGroup.orig_UpdateBossMemories orig, BossGroup self)
        {
            orig(self);
            UpdateMusic();
        }

        private void BossGroup_RememberBoss(On.RoR2.BossGroup.orig_RememberBoss orig, BossGroup self, CharacterMaster master)
        {
            orig(self, master);
            UpdateMusic();
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
            UpdateMusic();
        }

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
            Log($"{nameof(oldScene)}: {oldScene?.Name}");
            Log($"{nameof(runType)}: {runType}");
        }
    }
}
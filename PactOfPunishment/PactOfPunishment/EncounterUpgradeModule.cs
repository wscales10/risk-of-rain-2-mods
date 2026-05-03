using HG;
using PactOfPunishment.Conditions;
using PactOfPunishment.Waves.Infrastructure;
using RoR2;
using System;
using System.Linq;
using UnityEngine;

namespace PactOfPunishment
{
    public class EncounterUpgradeModule : Module
    {
        public delegate UpgradeEncounterStrategy? TryGetUpgradeStrategy(EncounterContext ctx, IWaveSelectionDefinition? waveSelectionDefinition);

        public static event TryGetUpgradeStrategy? OnInitializeEncounter;

        public override void Init()
        {
            On.RoR2.InfiniteTowerExplicitSpawnWaveController.Initialize += this.InfiniteTowerExplicitSpawnWaveController_Initialize;
            On.RoR2.InfiniteTowerWaveController.Initialize += this.InfiniteTowerWaveController_Initialize;
            On.EntityStates.MeridianEvent.Phase1.OnEnter += this.Phase1_OnEnter;
            On.EntityStates.MeridianEvent.Phase2.OnEnter += this.Phase2_OnEnter;
            On.EntityStates.MeridianEvent.Phase3.OnEnter += this.Phase3_OnEnter;
            On.RoR2.MeridianEventTriggerInteraction.HandleFSBFPhase2State += this.MeridianEventTriggerInteraction_HandleFSBFPhase2State;
        }

        internal static void DoUpgrade(Action orig, EncounterContext? ctx, IWaveSelectionDefinition? waveSelectionDefinition)
        {
            if (ctx == null)
            {
                orig();
                return;
            }

            var list = Utils.GetInvocationList(OnInitializeEncounter).Select(x => x(ctx, waveSelectionDefinition)).ToArray();

            foreach (var strategy in list)
            {
                strategy?.PreInitialise(ctx);
            }

            orig();

            foreach (var strategy in list)
            {
                strategy?.PostInitialise(ctx);
            }
        }

        private void Phase1_OnEnter(On.EntityStates.MeridianEvent.Phase1.orig_OnEnter orig, EntityStates.MeridianEvent.Phase1 self)
        {
            DoUpgrade(() => orig(self), new FalseSonBossFightContext(self), null);
        }

        private void Phase2_OnEnter(On.EntityStates.MeridianEvent.Phase2.orig_OnEnter orig, EntityStates.MeridianEvent.Phase2 self)
        {
            DoUpgrade(() => orig(self), new FalseSonBossFightContext(self), null);
        }

        private void Phase3_OnEnter(On.EntityStates.MeridianEvent.Phase3.orig_OnEnter orig, EntityStates.MeridianEvent.Phase3 self)
        {
            DoUpgrade(() => orig(self), new FalseSonBossFightContext(self), null);
        }

        private void MeridianEventTriggerInteraction_HandleFSBFPhase2State(On.RoR2.MeridianEventTriggerInteraction.orig_HandleFSBFPhase2State orig, MeridianEventTriggerInteraction self)
        {
            DoUpgrade(() => orig(self), self?.phase2CombatDirector?.GetComponent<EncounterContextHolder>()?.encounterContext, null);
        }

        private void InfiniteTowerExplicitSpawnWaveController_Initialize(On.RoR2.InfiniteTowerExplicitSpawnWaveController.orig_Initialize orig, InfiniteTowerExplicitSpawnWaveController self, int waveIndex, Inventory enemyInventory, GameObject spawnTargetObject)
        {
            this.Logger.LogDebug("Initializing explicit spawn wave controller...");
            orig(self, waveIndex, enemyInventory, spawnTargetObject);
        }

        private void InfiniteTowerWaveController_Initialize(On.RoR2.InfiniteTowerWaveController.orig_Initialize orig, InfiniteTowerWaveController self, int waveIndex, Inventory enemyInventory, GameObject spawnTarget)
        {
            DoUpgrade(() =>
            {
                this.Logger.LogDebug("Initializing wave controller...");
                orig(self, waveIndex, enemyInventory, spawnTarget);
            }, new InfiniteTowerWaveContext(self), Run.instance.GetComponent<SimulacrumWavesBehavior>().LastSelectedWaveSelectionDefinition);
        }
    }
}
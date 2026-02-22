using PactOfPunishment.Conditions;
using PactOfPunishment.Waves.Infrastructure;
using RoR2;
using System;
using System.Linq;
using UnityEngine;

namespace PactOfPunishment
{
    public class WaveUpgradesModule : Module
    {
        public delegate UpgradeWaveStrategy? TryGetUpgradeStrategy(InfiniteTowerWaveController waveController, IWaveSelectionDefinition? waveSelectionDefinition);

        public static event TryGetUpgradeStrategy? OnInitializeWave;

        public override void Init()
        {
            On.RoR2.InfiniteTowerExplicitSpawnWaveController.Initialize += this.InfiniteTowerExplicitSpawnWaveController_Initialize;
            On.RoR2.InfiniteTowerWaveController.Initialize += this.InfiniteTowerWaveController_Initialize;
        }

        private void InfiniteTowerExplicitSpawnWaveController_Initialize(On.RoR2.InfiniteTowerExplicitSpawnWaveController.orig_Initialize orig, InfiniteTowerExplicitSpawnWaveController self, int waveIndex, Inventory enemyInventory, GameObject spawnTargetObject)
        {
            this.Logger.LogDebug("Initializing explicit spawn wave controller...");
            orig(self, waveIndex, enemyInventory, spawnTargetObject);
        }

        private void InfiniteTowerWaveController_Initialize(On.RoR2.InfiniteTowerWaveController.orig_Initialize orig, InfiniteTowerWaveController self, int waveIndex, Inventory enemyInventory, GameObject spawnTarget)
        {
            var waveSelectionDefinition = Run.instance.GetComponent<SimulacrumWavesBehavior>().LastSelectedWaveSelectionDefinition;
            var list = OnInitializeWave?.GetInvocationList().Cast<TryGetUpgradeStrategy>().Select(x => x(self, waveSelectionDefinition)).ToArray() ?? Array.Empty<UpgradeWaveStrategy?>();

            foreach (var strategy in list)
            {
                strategy?.PreInitialise(self);
            }

            this.Logger.LogDebug("Initializing wave controller...");
            orig(self, waveIndex, enemyInventory, spawnTarget);

            foreach (var strategy in list)
            {
                strategy?.PostInitialise(self);
            }
        }
    }
}
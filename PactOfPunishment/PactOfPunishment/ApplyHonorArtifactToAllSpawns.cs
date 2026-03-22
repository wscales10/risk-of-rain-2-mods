using HG;
using PactOfPunishment.Conditions;
using UnityEngine.Networking;

namespace PactOfPunishment
{
    public class ApplyHonorArtifactToAllSpawns : Module
    {
        public override void Init()
        {
            On.RoR2.InfiniteTowerWaveController.Initialize += this.InfiniteTowerWaveController_Initialize;
        }

        private void InfiniteTowerWaveController_Initialize(On.RoR2.InfiniteTowerWaveController.orig_Initialize orig, RoR2.InfiniteTowerWaveController self, int waveIndex, RoR2.Inventory enemyInventory, UnityEngine.GameObject spawnTargetObject)
        {
            if (NetworkServer.active)
            {
                MonsterTracker.TrackCombatDirector(self.combatDirector);
                self.combatDirector.EnsureComponent<InfiniteTowerWaveSpawnListener>();
            }

            orig(self, waveIndex, enemyInventory, spawnTargetObject);
        }
    }
}
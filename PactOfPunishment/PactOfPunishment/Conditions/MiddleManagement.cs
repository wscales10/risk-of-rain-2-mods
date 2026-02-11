using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace PactOfPunishment.Conditions
{
    public sealed class MiddleManagement : DefaultConditionDef
    {
        public override int MaxRank => 1;

        public override int HeatPerRank => 2;

        public override void Init()
        {
            On.RoR2.InfiniteTowerExplicitSpawnWaveController.Initialize += InfiniteTowerExplicitSpawnWaveController_Initialize;
            On.RoR2.InfiniteTowerWaveController.Initialize += this.InfiniteTowerWaveController_Initialize;
        }

        private static bool IsMiniBossWave()
        {
            return Run.instance is InfiniteTowerRun run && !run.IsStageTransitionWave() && run.waveController.isBossWave; // TODO: this may change if I replace boss waves with separate stages sometimes
        }

        private static void InfiniteTowerExplicitSpawnWaveController_Initialize(On.RoR2.InfiniteTowerExplicitSpawnWaveController.orig_Initialize orig, InfiniteTowerExplicitSpawnWaveController self, int waveIndex, Inventory enemyInventory, GameObject spawnTargetObject)
        {
            Debug.Log("Initializing explicit spawn wave controller...");
            orig(self, waveIndex, enemyInventory, spawnTargetObject);
        }

        private void InfiniteTowerWaveController_Initialize(On.RoR2.InfiniteTowerWaveController.orig_Initialize orig, InfiniteTowerWaveController self, int waveIndex, Inventory enemyInventory, GameObject spawnTarget)
        {
            Debug.Log("Initializing wave controller...");
            orig(self, waveIndex, enemyInventory, spawnTarget);
            Debug.Log("Running custom wave initialization logic...");
            this.InitializeWave(self); // Need to do this after calling orig as the Lemurian one wants wave.rng. If you want to move some logic before orig, split into before and after.
        }

        private void InitializeWave(InfiniteTowerWaveController wave)
        {
            if (this.GetRank(wave) < 1)
            {
                return;
            }

            if (!IsMiniBossWave())
            {
                return;
            }

            if (wave.TryGetComponent<UpgradeWaveBehavior>(out var behavior) && behavior.strategy)
            {
                behavior.strategy!.UpgradeWave(wave);
                return;
            }

            if (wave is InfiniteTowerExplicitSpawnWaveController)
            {
                Debug.LogError("Not implemented");
                return;
            }

            wave.gameObject.AddComponent<MendingMiniMushrumWaveBehavior>().Init(wave);
        }

        public class MendingMiniMushrumWaveBehavior : MonoBehaviour
        {
            private CharacterSpawnCard miniMushrumSpawnCard;

            private bool isSpawningMushrum;

            public void Init(InfiniteTowerWaveController wave)
            {
                this.miniMushrumSpawnCard ??= Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/MiniMushroom/cscMiniMushroom.asset").WaitForCompletion();
                wave.combatDirector.onSpawnedWithDirectorServer.AddListener(this.OnSpawnedWithDirectorServer);
            }

            private void OnSpawnedWithDirectorServer(GameObject spawnedEntity, CombatDirector combatDirector)
            {
                if (!this.isSpawningMushrum)
                {
                    this.isSpawningMushrum = true;
                    combatDirector.Spawn(this.miniMushrumSpawnCard, DLC1Content.Elites.EarthHonor, spawnedEntity.transform, DirectorCore.MonsterSpawnDistance.Close, false);
                    this.isSpawningMushrum = false;
                }
                else
                {
                    var body = spawnedEntity.GetComponent<CharacterMaster>().GetBody();
                    body.inventory.GiveItemPermanent(RoR2Content.Items.CutHp, 2);
                    this.isSpawningMushrum = false;
                }
            }
        }
    }
}
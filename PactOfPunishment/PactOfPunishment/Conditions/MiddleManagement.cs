using HG;
using PactOfPunishment.Waves.Infrastructure;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace PactOfPunishment.Conditions
{
    public sealed class MiddleManagement : DefaultConditionDef // TODO: apply to mini-bosses summoned by Extreme Measures 4
    {
        public override int MaxRank => 1;

        public override int HeatPerRank => 2;

        public override void Init()
        {
            WaveUpgradesModule.OnInitializeWave += this.WaveUpgradesModule_OnInitializeWave;
            MendingMiniMushrumUpgradeStrategy.miniMushrumSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/Base/MiniMushroom/cscMiniMushroom.asset", this.Logger);
            On.RoR2.HealNearbyController.Tick += HealNearbyController_Tick;
        }

        private static bool IsMiniBossWave()
        {
            return Run.instance is InfiniteTowerRun run && !run.IsStageTransitionWave() && run.waveController.isBossWave; // TODO: this may change if I replace boss waves with separate stages sometimes
        }

        private static void HealNearbyController_Tick(On.RoR2.HealNearbyController.orig_Tick orig, HealNearbyController self)
        {
            if (NetworkServer.active && self.networkedBodyAttachment && self.networkedBodyAttachment.attachedBody && self.networkedBodyAttachment.attachedBody.TryGetComponent<SuperMendingBodyBehavior>(out var behavior) && !behavior.healNearbyControllers.Contains(self))
            {
                behavior.healNearbyControllers.Add(self);
                self.NetworkmaxTargets *= 3;
            }

            orig(self);
        }

        private UpgradeWaveStrategy? WaveUpgradesModule_OnInitializeWave(InfiniteTowerWaveController wave, IWaveSelectionDefinition? waveSelectionDefinition)
        {
            if (!this.IsEnabled(wave))
            {
                return null;
            }

            if (waveSelectionDefinition != null)
            {
                var upgradeStrategy = waveSelectionDefinition.GetUpgradeWaveStrategy(wave);

                if (upgradeStrategy && upgradeStrategy!.WaveUpgradeFilter == WaveUpgradeFilter.MiniBoss)
                {
                    return upgradeStrategy;
                }

                return null;
            }

            if (!IsMiniBossWave())
            {
                return null;
            }

            if (wave is InfiniteTowerExplicitSpawnWaveController)
            {
                this.Logger.LogError("Mini-boss upgrade not implemented");
                return null;
            }

            return ScriptableObject.CreateInstance<MendingMiniMushrumUpgradeStrategy>();
        }

        public class SuperMendingBodyBehavior : MonoBehaviour
        {
            public List<HealNearbyController> healNearbyControllers = new List<HealNearbyController>();
        }

        public class MendingMiniMushrumUpgradeStrategy : UpgradeWaveStrategy
        {
            internal static AssetPromise<CharacterSpawnCard> miniMushrumSpawnCard;

            private int counter = 0;

            private bool isSpawningMushrum;

            public override WaveUpgradeFilter WaveUpgradeFilter => WaveUpgradeFilter.MiniBoss;

            public override void PostInitialise(InfiniteTowerWaveController wave)
            {
                wave.combatDirector.onSpawnedWithDirectorServer.AddListener(this.OnSpawnedWithDirectorServer);
            }

            private void OnSpawnedWithDirectorServer(GameObject spawnedEntity, CombatDirector combatDirector)
            {
                if (!this.isSpawningMushrum)
                {
                    if (this.counter == 0)
                    {
                        this.isSpawningMushrum = true;
                        combatDirector.Spawn(miniMushrumSpawnCard.Value, DLC1Content.Elites.EarthHonor, spawnedEntity.transform, DirectorCore.MonsterSpawnDistance.Close, false, 0);
                        this.isSpawningMushrum = false;
                    }

                    this.counter = (this.counter + 1) % 3;
                }
                else
                {
                    var body = spawnedEntity.GetComponent<CharacterMaster>().GetBody();
                    body?.EnsureComponent<SuperMendingBodyBehavior>();
                    Utils.ScaleDeathRewards(body, 0);

                    // TODO: ensure monsters spawned by pact of punishment conditions do not reward the player for killing them.

                    this.isSpawningMushrum = false;
                }
            }
        }
    }
}
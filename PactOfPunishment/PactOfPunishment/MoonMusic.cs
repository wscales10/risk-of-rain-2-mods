using RoR2;
using RoR2.WwiseUtils;
using UnityEngine;

namespace PactOfPunishment
{
    public class MoonMusic : Module
    {
        private StateSetter bossPhaseSetter;

        public static float BossSpawnDelay { get; set; } = 3;

        public static MoonMusic Instance { get; } = new MoonMusic();

        internal static bool IsMoonStage => SceneCatalog.GetSceneDefForCurrentScene()?.cachedName == "itmoon";

        public void PlayBossTrack(InfiniteTowerWaveController waveController)
        {
            var musicOverride = waveController.gameObject.AddComponent<MusicTrackOverride>();
            musicOverride.track = MusicTrackCatalog.FindMusicTrackDef("muSong25");

            if (PhaseCounter.instance && PhaseCounter.instance.phase == 3)
            {
                this.SetPhase3();
            }
            else
            {
                Debug.Log("Queueing Mithrix Phase 1 music");
                this.bossPhaseSetter.valueId = AkSoundEngine.GetIDFromString("phase1");
            }

            waveController.onAllEnemiesDefeatedServer += this.OnAllEnemiesDefeatedServer;
        }

        public override void Init()
        {
            On.RoR2.InfiniteTowerWaveController.Initialize += this.InfiniteTowerWaveController_Initialize;
            On.RoR2.MusicController.InitializeEngineDependentValues += this.MusicController_InitializeEngineDependentValues;
            On.RoR2.MusicController.FlushValuesToEngine += this.MusicController_FlushValuesToEngine;
            On.EntityStates.InfiniteTowerSafeWard.Travelling.OnEnter += this.Travelling_OnEnter;
        }

        internal void SetPhase3()
        {
            Debug.Log("Queueing Mithrix Phase 3 music");
            this.bossPhaseSetter.valueId = AkSoundEngine.GetIDFromString("phase3");
        }

        private void Travelling_OnEnter(On.EntityStates.InfiniteTowerSafeWard.Travelling.orig_OnEnter orig, EntityStates.InfiniteTowerSafeWard.Travelling self)
        {
            orig(self);

            if (Run.instance is InfiniteTowerRun { waveController: InfiniteTowerWaveController wave } && wave.TryGetComponent<MusicTrackOverride>(out var musicTrackOverride))
            {
                musicTrackOverride.enabled = false;
            }
        }

        private void MusicController_InitializeEngineDependentValues(On.RoR2.MusicController.orig_InitializeEngineDependentValues orig, MusicController self)
        {
            orig(self);
            this.bossPhaseSetter = new StateSetter("bossPhase");
        }

        private void MusicController_FlushValuesToEngine(On.RoR2.MusicController.orig_FlushValuesToEngine orig, MusicController self)
        {
            orig(self);
            this.bossPhaseSetter.FlushIfChanged();
        }

        private void InfiniteTowerWaveController_Initialize(On.RoR2.InfiniteTowerWaveController.orig_Initialize orig, InfiniteTowerWaveController self, int waveIndex, Inventory enemyInventory, GameObject spawnTarget)
        {
            if (IsMoonStage && !self.GetComponent<MusicTrackOverride>())
            {
                var run = Run.instance as InfiniteTowerRun;

                if (run)
                {
                    var stageSecondHalfTrackOverride = run!.safeWardController.GetComponent<MusicTrackOverride>();

                    if (self.isBossWave)
                    {
                        if (stageSecondHalfTrackOverride)
                        {
                            stageSecondHalfTrackOverride.enabled = false;
                        }

                        this.Logger.LogDebug("Queueing Mithrix Phase 2 music");
                        this.bossPhaseSetter.valueId = AkSoundEngine.GetIDFromString("phase2");
                        self.onAllEnemiesDefeatedServer += this.OnAllEnemiesDefeatedServer;
                    }
                    else if (((waveIndex - 1) % run!.stageTransitionPeriod) * 2 >= run.stageTransitionPeriod)
                    {
                        var safeWardController = run.safeWardController;

                        if (stageSecondHalfTrackOverride)
                        {
                            stageSecondHalfTrackOverride.enabled = true;
                        }
                        else
                        {
                            stageSecondHalfTrackOverride = safeWardController.gameObject.AddComponent<MusicTrackOverride>();
                            stageSecondHalfTrackOverride.track = MusicTrackCatalog.FindMusicTrackDef("muSong25");
                            this.bossPhaseSetter.valueId = AkSoundEngine.GetIDFromString("phase4");
                        }
                    }
                }
            }

            orig(self, waveIndex, enemyInventory, spawnTarget);
        }

        private void OnAllEnemiesDefeatedServer(InfiniteTowerWaveController wave)
        {
            wave.onAllEnemiesDefeatedServer -= this.OnAllEnemiesDefeatedServer;
            this.bossPhaseSetter.valueId = AkSoundEngine.GetIDFromString("bossDead");

            if (Run.instance is InfiniteTowerRun run && !run.IsStageTransitionWave() && IsMoonStage && wave.TryGetComponent<MusicTrackOverride>(out var musicTrackOverride))
            {
                musicTrackOverride.enabled = false;
            }
        }
    }
}
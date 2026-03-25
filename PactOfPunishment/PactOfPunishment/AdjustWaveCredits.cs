using HG;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using static PactOfPunishment.IncreaseSpawnRateWhileThereAreNoMonsters;

namespace PactOfPunishment
{
    // TODO: integrate better with other modules which hook Initialize
    public class AdjustWaveCredits : Module
    {
        public override void Init()
        {
            IL.RoR2.InfiniteTowerWaveController.Initialize += Utils.HookIL(InfiniteTowerWaveController_Initialize);
            On.RoR2.InfiniteTowerWaveController.Initialize += this.InfiniteTowerWaveController_Initialize;
        }

        private static void InfiniteTowerWaveController_Initialize(ILCursor c)
        {
            c.GotoNext(x => x.MatchCall<InfiniteTowerWaveController>($"set_{nameof(InfiniteTowerWaveController.totalWaveCredits)}"));
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<float, InfiniteTowerWaveController, float>>((orig, self) =>
            {
                var spawnRateMultiplier = self.combatDirector.EnsureComponent<SimulacrumCombatDirectorSpawnRateMultiplier>();

                // TODO: check this
                if (self.isBossWave)
                {
                    spawnRateMultiplier.TotalWaveCreditsMultiplier = 1;
                }
                else
                {
                    spawnRateMultiplier.TotalWaveCreditsMultiplier = 0.5f;
                }

                return orig * spawnRateMultiplier.TotalWaveCreditsMultiplier;
            });
        }

        private void InfiniteTowerWaveController_Initialize(On.RoR2.InfiniteTowerWaveController.orig_Initialize orig, InfiniteTowerWaveController self, int waveIndex, Inventory enemyInventory, UnityEngine.GameObject spawnTarget)
        {
            var spawnRateMultiplier = self.combatDirector.EnsureComponent<SimulacrumCombatDirectorSpawnRateMultiplier>();

            if (!self.isBossWave)
            {
                float wavePeriodSecondsMultiplier = 0.5f;
                spawnRateMultiplier.WavePeriodSecondsMultiplier = wavePeriodSecondsMultiplier;
                float secondsRemovedFromWave = self.wavePeriodSeconds * (1 - wavePeriodSecondsMultiplier);
                self.wavePeriodSeconds *= wavePeriodSecondsMultiplier;
                self.secondsBeforeSuddenDeath += secondsRemovedFromWave;
            }
            else
            {
                spawnRateMultiplier.WavePeriodSecondsMultiplier = 1; // TODO: Apply also to boss waves? If so, be careful of interaction with directors which use money waves
            }

            orig(self, waveIndex, enemyInventory, spawnTarget);
        }
    }
}
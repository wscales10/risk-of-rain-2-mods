using HG;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using UnityEngine;

namespace PactOfPunishment
{
    public partial class IncreaseSpawnRateWhileThereAreNoMonsters : Module
    {
        public override void Init()
        {
            IL.RoR2.CombatDirector.Simulate += Utils.HookIL(CombatDirector_Simulate);
            IL.RoR2.InfiniteTowerWaveController.FixedUpdate += Utils.HookIL(InfiniteTowerWaveController_FixedUpdate);
            On.RoR2.CombatDirector.AttemptSpawnOnTarget += CombatDirector_AttemptSpawnOnTarget; // TODO: include all monster spawns?
        }

        private static void InfiniteTowerWaveController_FixedUpdate(ILCursor c)
        {
            while (c.TryGotoNext(MoveType.After, x => x.MatchLdfld<InfiniteTowerWaveController>(nameof(InfiniteTowerWaveController.creditsPerSecond))))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<float, InfiniteTowerWaveController, float>>(AdjustSimulacrumWaveCreditsPerSecond);
            }
        }

        private static float AdjustSimulacrumWaveCreditsPerSecond(float value, InfiniteTowerWaveController self)
        {
            return (self.combatDirector?.EnsureComponent<SimulacrumCombatDirectorSpawnRateMultiplier>().CreditGainRateMultiplier ?? 1) * value;
        }

        private bool CombatDirector_AttemptSpawnOnTarget(On.RoR2.CombatDirector.orig_AttemptSpawnOnTarget orig, RoR2.CombatDirector self, Transform spawnTarget, RoR2.DirectorPlacementRule.PlacementMode placementMode)
        {
            if (orig(self, spawnTarget, placementMode))
            {
                if (CombatDirector.cvDirectorCombatEnableInternalLogs.value)
                {
                    this.Logger.LogDebug($"Successful Combat Director spawn attempt {Run.instance.GetRunStopwatch()}s into the run.");
                }

                if (self.TryGetComponent<SimulacrumCombatDirectorSpawnRateMultiplier>(out var behavior))
                {
                    behavior.CreditGainRateMultiplier = 1;
                }

                if (Run.instance.TryGetComponent<RunSpawnCounter>(out var counter) && counter.enabled)
                {
                    counter.SpawnedMonsters++;
                }

                return true;
            }
            else
            {
                if (CombatDirector.cvDirectorCombatEnableInternalLogs.value)
                {
                    this.Logger.LogDebug($"Failed combat Director spawn attempt {Run.instance.GetRunStopwatch()}s into the run.");
                }

                return false;
            }
        }

        private void CombatDirector_Simulate(ILCursor c)
        {
            while (c.TryGotoNext(
                x => x.MatchCallvirt<Xoroshiro128Plus>(nameof(Xoroshiro128Plus.RangeFloat)),
                x => x.MatchAdd(),
                x => x.MatchCall<CombatDirector>($"set_{nameof(CombatDirector.monsterSpawnTimer)}")))
            {
                c.Index++;
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<float, CombatDirector, float>>(this.AdjustSpawnInterval);
            }
        }

        private float AdjustSpawnInterval(float originalSpawnInterval, CombatDirector self)
        {
            if (!self.TryGetComponent<SimulacrumCombatDirectorSpawnRateMultiplier>(out var behavior))
            {
                this.Logger.LogDebug($"Setting combat director spawn attempt interval to {originalSpawnInterval}");
                return originalSpawnInterval;
            }

            if (self.combatSquad && self.combatSquad.memberCount == 0)
            {
                behavior.CreditGainRateMultiplier = Mathf.Max(1, originalSpawnInterval / 0.5f);
            }
            else
            {
                behavior.CreditGainRateMultiplier = 1;
            }

            float multiplier = behavior.SpawnAttemptIntervalMultiplier;
            float output = originalSpawnInterval * multiplier;

            if (Mathf.Approximately(multiplier, 1))
            {
                this.Logger.LogDebug($"Setting combat director spawn attempt interval to {output}");
            }
            else
            {
                this.Logger.LogDebug($"Setting combat director spawn attempt interval to {originalSpawnInterval} / {1 / multiplier} = {output}");
            }

            return output;
        }
    }
}
using HG;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using UnityEngine;

namespace PactOfPunishment
{
    public class IncreaseSpawnRateWhileThereAreNoMonsters : Module
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
                c.EmitDelegate<Func<float, InfiniteTowerWaveController, float>>((value, self) => (self.combatDirector?.EnsureComponent<SimulacrumCombatDirectorSpawnRateMultiplier>().SpawnRateMultiplier ?? 1) * value);
            }
        }

        private static bool CombatDirector_AttemptSpawnOnTarget(On.RoR2.CombatDirector.orig_AttemptSpawnOnTarget orig, RoR2.CombatDirector self, Transform spawnTarget, RoR2.DirectorPlacementRule.PlacementMode placementMode)
        {
            if (orig(self, spawnTarget, placementMode))
            {
                if (self.TryGetComponent<SimulacrumCombatDirectorSpawnRateMultiplier>(out var behavior))
                {
                    behavior.SpawnRateMultiplier = 1;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        private static void CombatDirector_Simulate(ILCursor c)
        {
            c.GotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdarg(0),
                x => x.MatchCall<CombatDirector>($"get_{nameof(CombatDirector.monsterSpawnTimer)}"),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<CombatDirector>(nameof(CombatDirector.rng)),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<CombatDirector>(nameof(CombatDirector.minRerollSpawnInterval)),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<CombatDirector>(nameof(CombatDirector.maxRerollSpawnInterval)),
                x => x.MatchCallvirt<Xoroshiro128Plus>(nameof(Xoroshiro128Plus.RangeFloat))
            );

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<float, CombatDirector, float>>((rerollSpawnInterval, self) =>
            {
                if (self.combatSquad && self.combatSquad.memberCount == 0 && self.TryGetComponent<SimulacrumCombatDirectorSpawnRateMultiplier>(out var behavior))
                {
                    var multiplier = Mathf.Max(1, rerollSpawnInterval / 0.5f);
                    behavior.SpawnRateMultiplier = multiplier;
                    return rerollSpawnInterval / multiplier;
                }
                else
                {
                    return rerollSpawnInterval;
                }
            });
        }

        public class SimulacrumCombatDirectorSpawnRateMultiplier : MonoBehaviour
        {
            public float SpawnRateMultiplier = 1;
        }
    }
}
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using System.Linq;
using static RoR2.InfiniteTowerWaveCategory;

namespace PactOfPunishment.Waves
{
    public class SimulacrumWaves : Module
    {
        internal static event Action<HealthComponent, DamageInfo>? OnTakeNonZeroDamageGlobal;

        public override void Init()
        {
            IL.RoR2.InfiniteTowerWaveCategory.SelectWavePrefab += Utils.HookIL(InfiniteTowerWaveCategory_SelectWavePrefab);
            IL.RoR2.HealthComponent.TakeDamageProcess += Utils.HookIL(HealthComponent_TakeDamageProcess);
            On.RoR2.Run.Start += Run_Start;
        }

        private static void Run_Start(On.RoR2.Run.orig_Start orig, Run self)
        {
            orig(self);

            if (self is InfiniteTowerRun)
            {
                self.gameObject.AddComponent<SimulacrumWavesBehavior>();
            }
        }

        private static void HealthComponent_TakeDamageProcess(ILCursor c)
        {
            c.GotoNext(MoveType.AfterLabel,
                x => x.MatchLdarg(0),
                x => x.MatchLdflda<HealthComponent>(nameof(HealthComponent.itemCounts)),
                x => x.MatchLdfld<HealthComponent.ItemCounts>(nameof(HealthComponent.ItemCounts.thorns)),
                x => x.MatchLdcI4(0),
                x => x.MatchBle(out _)
            );
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate<Action<HealthComponent, DamageInfo>>((self, damageInfo) =>
            {
                OnTakeNonZeroDamageGlobal?.Invoke(self, damageInfo);
            });
        }

        private static void InfiniteTowerWaveCategory_SelectWavePrefab(ILCursor c)
        {
            c.GotoNext(x => x.MatchLdarg(0),
                x => x.MatchLdfld<InfiniteTowerWaveCategory>(nameof(InfiniteTowerWaveCategory.wavePrefabs)),
                x => x.MatchStloc(0));
            c.Index += 2;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<WeightedWave[], InfiniteTowerWaveCategory, WeightedWave[]>>((wavePrefabs, self) =>
            {
                if (Run.instance.TryGetComponent<SimulacrumWavesBehavior>(out var behavior) && behavior.TryOverrideWeightedSelection(self))
                {
                    return self.weightedSelection.choices.Where(x => x.value).Select(x => new WeightedWave { wavePrefab = x.value, weight = x.weight }).ToArray();
                }

                return wavePrefabs;
            });
        }
    }
}
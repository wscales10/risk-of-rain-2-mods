using Mono.Cecil.Cil;
using MonoMod.Cil;
using PactOfPunishment.Waves.Stage1;
using PactOfPunishment.Waves.Stage1.Halcyonites;
using PactOfPunishment.Waves.Stage2;
using PactOfPunishment.Waves.Stage3;
using RoR2;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using static RoR2.InfiniteTowerWaveCategory;

namespace PactOfPunishment.Waves.Infrastructure
{
    public class SimulacrumWavesModule : Module
    {
        private SimulacrumWavesModule()
        {
        }

        internal static event Action<HealthComponent, DamageInfo>? OnTakeNonZeroDamageGlobal;

        public static SimulacrumWavesModule Instance { get; } = new SimulacrumWavesModule();

        public SimulacrumWaveDefinitions Cache { get; } = new SimulacrumWaveDefinitions();

        public override void Init()
        {
            IL.RoR2.InfiniteTowerWaveCategory.SelectWavePrefab += Utils.HookIL(InfiniteTowerWaveCategory_SelectWavePrefab);
            IL.RoR2.HealthComponent.TakeDamageProcess += Utils.HookIL(HealthComponent_TakeDamageProcess);
            On.RoR2.Run.Start += Run_Start;
            On.RoR2.InfiniteTowerRun.MoveSafeWard += InfiniteTowerRun_MoveSafeWard;
            On.RoR2.InfiniteTowerBossWaveController.OnTimerExpire += InfiniteTowerBossWaveController_OnTimerExpire;
            On.RoR2.InfiniteTowerExplicitSpawnWaveController.OnTimerExpire += InfiniteTowerExplicitSpawnWaveController_OnTimerExpire;

            Content.Elites.NerfedPoison = Projectilers.AllMalachiteWaveStrategy.MakeEliteDef();
            Content.EliteTiers.NerfedPoisonTier = Projectilers.AllMalachiteWaveStrategy.MakeEliteTierDef();

            // Stage 1
            this.Cache.Add<RunaldAndKjaro>();
            this.Cache.Add<SolusControlUnit>();
            this.Cache.Add<ImpOverlord>();

            this.Cache.Add<Halcyonite1>();

            // Stage 2
            this.Cache.Add<WormAndDistributor>();
            this.Cache.Add<Projectilers>();

            this.Cache.Add<Summoner.Summoner>();

            // Stage 3
            this.Cache.Add<Mithrix>();
            this.Cache.Add<Summoner2>();

            this.Cache.Add<MithrixWithHalcyonite>();
        }

        private static void InfiniteTowerExplicitSpawnWaveController_OnTimerExpire(On.RoR2.InfiniteTowerExplicitSpawnWaveController.orig_OnTimerExpire orig, InfiniteTowerExplicitSpawnWaveController self)
        {
            if (self.waveIndex % 5 == 0)
            {
                orig(self);
            }
            else
            {
                if (!NetworkServer.active)
                {
                    Debug.LogWarning("[Server] function 'System.Void RoR2.InfiniteTowerWaveController::OnTimerExpire()' called on client");
                    return;
                }
                self.MarkAsFinished();
            }
        }

        private static void InfiniteTowerBossWaveController_OnTimerExpire(On.RoR2.InfiniteTowerBossWaveController.orig_OnTimerExpire orig, InfiniteTowerBossWaveController self)
        {
            if (self.waveIndex % 5 == 0)
            {
                orig(self);
            }
            else
            {
                if (!NetworkServer.active)
                {
                    Debug.LogWarning("[Server] function 'System.Void RoR2.InfiniteTowerWaveController::OnTimerExpire()' called on client");
                    return;
                }
                self.MarkAsFinished();
            }
        }

        private static void InfiniteTowerRun_MoveSafeWard(On.RoR2.InfiniteTowerRun.orig_MoveSafeWard orig, InfiniteTowerRun self)
        {
            if (self.waveIndex % 5 == 0)
            {
                orig(self);
            }
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
                OnTakeNonZeroDamageGlobal?.Invoke(self, damageInfo); // TODO: replace with IOnTakeDamageServerReceiver
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
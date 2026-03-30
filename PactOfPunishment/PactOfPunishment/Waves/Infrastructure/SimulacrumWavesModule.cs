using EntityStates.DefectiveUnit;
using EntityStates.InfiniteTowerSafeWard;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using PactOfPunishment.MonsterSpawnDistance;
using PactOfPunishment.Waves.Stage1;
using PactOfPunishment.Waves.Stage1.Halcyonites.Halcyonite1;
using PactOfPunishment.Waves.Stage1.Halcyonites.Halcyonite2;
using PactOfPunishment.Waves.Stage1.Halcyonites.Halcyonite3;
using PactOfPunishment.Waves.Stage2;
using PactOfPunishment.Waves.Stage3;
using PactOfPunishment.Waves.Stage4;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.WwiseUtils;
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
            IL.RoR2.MusicController.PickCurrentTrack += Utils.HookIL(MusicController_PickCurrentTrack);
            On.RoR2.MusicController.UpdateTeleporterParameters += this.MusicController_UpdateTeleporterParameters;

            On.EntityStates.Scorchling.ScorchlingBreach.OnEnter += this.ScorchlingBreach_OnEnter;

            Content.Elites.NerfedPoison = Projectilers.AllMalachiteWaveStrategy.MakeEliteDef();
            Content.EliteTiers.NerfedPoisonTier = Projectilers.AllMalachiteWaveStrategy.MakeEliteTierDef();
            Content.MonsterSpawnDistances.WithinZone = MonsterSpawnDistanceApi.RegisterMonsterSpawnDistance(() => (8, 55)); // TODO: add ModdedMonsterSpawnDistance class and get distance based on (maximum) zone radius?
            Content.Items.ChiefBossMarker = AddItem(ItemTier.NoTier, nameof(Content.Items.ChiefBossMarker), item =>
            {
                item.hidden = true;
                item.canRemove = false;
                item.tags = new ItemTag[] { ItemTag.CannotSteal, ItemTag.HiddenForDroneBuffIcon };
            });

            Summoner2BossFightBehavior.eggSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/Junk/Incubator/cscParentPod.asset"); // TODO: move to its own module?
            Summoner2BossFightBehavior.parentSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/Base/Parent/cscParent.asset"); // TODO: hook global spawn card event instead and get spawn card from spawn request

            Utils.OnLoad<GameObject>("RoR2/Base/Titan/TitanGoldPreFistProjectile.prefab", x => FistsController.zoneProjectilePrefab = x);

            IL.EntityStates.DefectiveUnit.DenialProjectile.FireProjectile += Utils.HookIL(DenialProjectile_FireProjectile);

            // Stage 1
            this.Cache.Add<RunaldAndKjaro>();
            this.Cache.Add<SolusControlUnit>();
            this.Cache.Add<ImpOverlord>();

            this.Cache.Add<Halcyonite1>();
            this.Cache.Add<Halcyonite2>();
            this.Cache.Add<Halcyonite3>();

            // Stage 2
            this.Cache.Add<WormAndDistributor>();
            this.Cache.Add<Projectilers>();

            this.Cache.Add<Summoner.Summoner>();

            // Stage 3
            this.Cache.Add<Mithrix>();
            this.Cache.Add<Summoner2>();

            this.Cache.Add<MithrixWithHalcyonite>();

            // Stage 4
            this.Cache.Add<Aurelionite>();
            this.Cache.Add<BlazingElderLemurian>();
            this.Cache.Add<Gup>();
            this.Cache.Add<Invalidator>();
        }

        private static ItemDef AddItem(ItemTier tier, string name, Action<ItemDef>? setup = null)
        {
            var customItem = new CustomItem(
                name,
                $"ITEM_{name.ToUpperInvariant()}_NAME",
                $"ITEM_{name.ToUpperInvariant()}_DESC",
                $"ITEM_{name.ToUpperInvariant()}_LORE",
                $"ITEM_{name.ToUpperInvariant()}_PICKUP",
                null,
                null,
                Array.Empty<ItemTag>(),
                tier,
                false,
                true,
                null,
                null);
            customItem.ItemDef.deprecatedTier = tier; // Have to set this for untiered items, and seems sensible to set it for all. I guess I could fix the bug that causes it to be required...
            setup?.Invoke(customItem.ItemDef!);

            if (!ItemAPI.Add(customItem))
            {
                throw new InvalidOperationException();
            }

            return customItem.ItemDef!;
        }

        private static void DenialProjectile_FireProjectile(ILCursor c)
        {
            c.GotoLast(x => x.MatchCallvirt<ProjectileManager>(nameof(ProjectileManager.FireProjectile)));
            c.Emit(OpCodes.Dup);
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<FireProjectileInfo, DenialProjectile>>((orig, self) =>
            {
                if (self.characterBody?.GetComponent<Stage4.Invalidator.BodyBehavior>())
                {
                    for (int i = 1; i < 6; i++)
                    {
                        var copy = orig;
                        copy.rotation *= Quaternion.Euler(Vector3.up * 60 * i);
                        ProjectileManager.instance.FireProjectile(copy);
                    }
                }
            });
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

        private static bool? IsSimulacrumBossAlive()
        {
            if (!(Run.instance is InfiniteTowerRun run && run.waveController && run.waveController.isBossWave))
            {
                return null;
            }

            var safeWardState = Utils.GetSafeWardState();
            if (!(safeWardState is Active || safeWardState is Unburrow || safeWardState is AwaitingPortalUse))
            {
                return null;
            }

            return !run.waveController.haveAllEnemiesBeenDefeated;
        }

        private void ScorchlingBreach_OnEnter(On.EntityStates.Scorchling.ScorchlingBreach.orig_OnEnter orig, EntityStates.Scorchling.ScorchlingBreach self)
        {
            if (self.GetComponent<WormAndDistributor.WormMiniBossInfo.WormBossBodyBehavior>())
            {
                self.crackToBreachTime *= 0.75f;
                self.breachToBurrow *= 0.5f;
            }

            orig(self);
        }

        private void MusicController_UpdateTeleporterParameters(On.RoR2.MusicController.orig_UpdateTeleporterParameters orig, MusicController self, TeleporterInteraction teleporter, Transform cameraTransform, CharacterBody targetBody)
        {
            orig(self, teleporter, cameraTransform, targetBody);

            if (IsSimulacrumBossAlive() == false)
            {
                self.stBossStatus.valueId = CommonWwiseIds.dead;
            }
        }

        private static void MusicController_PickCurrentTrack(ILCursor c)
        {
            c.GotoNext(MoveType.AfterLabel,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<MusicController>(nameof(MusicController.enableMusicSystem)),
                x => x.MatchBrfalse(out _));
            c.Emit(OpCodes.Ldloc_1);
            c.EmitDelegate<Func<bool, bool>>(orig =>
            {
                if (orig)
                {
                    return true;
                }

                return IsSimulacrumBossAlive() != null; // TODO: use override behavior instead and choose boss tracks more carefully?
            });
            c.Emit(OpCodes.Stloc_1);
        }
    }
}
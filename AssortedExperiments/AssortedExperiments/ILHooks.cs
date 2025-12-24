using BepInEx.Logging;
using EntityStates.FalseSonBoss;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.Artifacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AssortedExperiments
{
    internal class ILHooks : Hooks
    {
        public ILHooks(ManualLogSource logger, Settings settings, HashSet<SceneDirector> waitingForScrapper) : base(logger, settings, waitingForScrapper)
        {
        }

        public static void GlobalEventManager_OnCharacterHitGroundServer(ILContext il)
        {
            var c = new ILCursor(il);
            c.GotoNext(
                x => x.MatchLdloc(7),
                x => x.MatchLdloc(6),
                x => x.MatchLdarg(1),
                x => x.MatchCallvirt<CharacterBody>($"get_{nameof(CharacterBody.maxHealth)}"),
                x => x.MatchMul(),
                x => x.MatchStfld<DamageInfo>(nameof(DamageInfo.damage)));
            c.Index += 2;
            c.RemoveRange(2);
            c.Emit(OpCodes.Ldloc_S, (byte)5);
            c.EmitDelegate<Func<HealthComponent, float>>(GetEffectiveMaxHealthForFallDamage);
        }

        // TODO: consider doing this in the same way as the halcyonite shrine or vice versa
        public void SkyJumpDeathState_GiveColossusItem(ILContext il)
        {
            var c = new ILCursor(il);

            c.GotoNext(
                x => x.MatchLdfld<SkyJumpDeathState>(nameof(SkyJumpDeathState.dtColossusBuffDropTable)),
                x => x.MatchLdloc(1),
                x => x.MatchCall<PickupPickerController>(nameof(PickupPickerController.GenerateOptionsFromDropTable)),
                x => x.MatchStfld<GenericPickupController.CreatePickupInfo>(nameof(GenericPickupController.CreatePickupInfo.pickerOptions)));

            c.Index += 1;
            c.EmitDelegate(this.RandomlyTransformDropTable());
        }

        public void RouletteChestController_GenerateEntriesServer(ILContext il)
        {
            // TODO: specific to interactor or nearby players or something? not anything which will
            // encourage gimmicky behavior like kissing the chest...
            var c = new ILCursor(il);

            while (c.TryGotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<RouletteChestController>(nameof(RouletteChestController.dropTable)),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<RouletteChestController>(nameof(RouletteChestController.rng)),
                x => x.MatchCallvirt<PickupDropTable>(nameof(PickupDropTable.GeneratePickup))))
            {
                c.Index += 2;
                c.EmitDelegate(this.RandomlyTransformDropTable());
            }
        }

        public void OptionChestBehavior_Roll(ILContext il)
        {
            // TODO: make this depend on the interactor, or delay the shenanigans until someone actually opens the orb
            var c = new ILCursor(il);

            c.GotoNext(x => x.MatchLdfld<OptionChestBehavior>(nameof(OptionChestBehavior.dropTable)));
            c.Index += 1;
            c.EmitDelegate(this.RandomlyTransformDropTable());
        }

        public void InfiniteTowerWaveController_DropRewards(ILContext il)
        {
            var c = new ILCursor(il);

            c.GotoNext(
                x => x.MatchLdfld<InfiniteTowerWaveController>(nameof(InfiniteTowerWaveController.rewardDropTable)),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<InfiniteTowerWaveController>(nameof(InfiniteTowerWaveController.rng)),
                x => x.MatchCall<PickupPickerController>(nameof(PickupPickerController.GenerateOptionsFromDropTable)));

            c.Index += 1;
            c.EmitDelegate(this.RandomlyTransformDropTable());
        }

        public void ArenaMissionController_EndRound(ILContext il)
        {
            var c = new ILCursor(il);

            c.GotoNext(
                x => x.MatchLdloc(6),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<ArenaMissionController>(nameof(ArenaMissionController.numRewardOptions)),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<ArenaMissionController>(nameof(ArenaMissionController.rng)),
                x => x.MatchLdcI4(1),
                x => x.MatchCall<PickupDropTable>(nameof(PickupDropTable.GenerateDistinctPickups)));

            c.MoveAfterLabels();
            c.EmitDelegate(this.RandomlyTransformDropTable());
        }

        public void ShrineChanceBehavior_AddShrineStack(ILContext il)
        {
            var c = new ILCursor(il);

            while (c.TryGotoNext(
                x => x.MatchCallvirt<PickupDropTable>(nameof(PickupDropTable.GeneratePickup))))
            {
                c.Index -= 2;
                c.Emit(OpCodes.Ldarg_1);
                c.EmitDelegate<Func<Interactor, PlayerCharacterMasterController?>>(x => x.GetComponent<CharacterBody>()?.master?.playerCharacterMasterController);
                c.EmitDelegate(this.RandomlyTransformDropTableForPlayer());
                c.Index += 3;
            }
        }

        public void SacrificeArtifactManager_OnServerCharacterDeath(ILContext il)
        {
            var c = new ILCursor(il);

            c.GotoNext(
                x => x.MatchLdsfld(typeof(SacrificeArtifactManager).FullName, nameof(SacrificeArtifactManager.dropTable)),
                x => x.MatchLdsfld(typeof(SacrificeArtifactManager).FullName, nameof(SacrificeArtifactManager.treasureRng)),
                x => x.MatchCallvirt<PickupDropTable>(nameof(PickupDropTable.GeneratePickup)),
                x => x.MatchStloc(0));

            c.Index += 1;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate(this.GetAttackingPlayerFromDamageReport());
            c.EmitDelegate(this.RandomlyTransformDropTableForPlayer());
        }

        public void GlobalEventManager_OnCharacterDeath(ILContext il)
        {
            var c = new ILCursor(il);

            while (c.TryGotoNext(
                x => x.MatchLdsfld(typeof(GlobalEventManager.CommonAssets).FullName, nameof(GlobalEventManager.CommonAssets.dtSonorousEchoPath))))
            {
                c.Index += 1;
                c.Emit(OpCodes.Ldarg_1);
                c.EmitDelegate(this.GetAttackingPlayerFromDamageReport());
                c.EmitDelegate(this.RandomlyTransformDropTableForPlayer());
            }
        }

        public void EquipmentSlot_FireRecycle(ILContext il)
        {
            var c = new ILCursor(il);

            c.GotoNext(
                x => x.MatchLdloc(2),
                x => x.MatchCallvirt<Xoroshiro128Plus>(nameof(Xoroshiro128Plus.NextElementUniform)));

            c.Index += 1;
            c.Emit(OpCodes.Ldarg_0);
            c.Emit<EquipmentSlot>(OpCodes.Call, $"get_{nameof(EquipmentSlot.characterBody)}");
            c.EmitDelegate<Func<PickupIndex[], CharacterBody, PickupIndex[]>>(this.RandomlyTransformPickupIndexArray);
        }

        public void BossGroup_DropRewards(ILContext il)
        {
            var c = new ILCursor(il);

            c.GotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<BossGroup>(nameof(BossGroup.dropTable)),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<BossGroup>(nameof(BossGroup.rng)),
                x => x.MatchCallvirt<PickupDropTable>(nameof(PickupDropTable.GeneratePickup)),
                x => x.MatchStloc(1),
                x => x.MatchBr(out _));

            c.Index += 2;
            c.EmitDelegate(this.RandomlyTransformDropTable());
        }

        public void ChestBehavior_Roll(ILContext il)
        {
            // TODO: Use MonoDetour
            // TODO: Check just the interactor's inventory, not all players' inventories.
            var c = new ILCursor(il);

            c.GotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<ChestBehavior>(nameof(ChestBehavior.dropTable)),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<ChestBehavior>(nameof(ChestBehavior.rng)),
                x => x.MatchCallvirt<PickupDropTable>(nameof(PickupDropTable.GeneratePickup)),
                x => x.MatchCall<ChestBehavior>($"set_{nameof(ChestBehavior.currentPickup)}"),
                x => x.MatchRet());

            c.Index += 3;
            c.EmitDelegate(this.RandomlyTransformDropTable());
        }

        public void ShopTerminalBehavior_GenerateNewPickupServer_bool(ILContext il)
        {
            var c = new ILCursor(il);

            c.GotoNext(
                i => i.MatchLdarg(0),
                i => i.MatchLdfld<ShopTerminalBehavior>(nameof(ShopTerminalBehavior.dropTable)),
                i => i.MatchLdarg(0),
                i => i.MatchLdfld<ShopTerminalBehavior>(nameof(ShopTerminalBehavior.rng)),
                i => i.MatchCallvirt<PickupDropTable>(nameof(PickupDropTable.GeneratePickup)),
                i => i.MatchStloc(0),
                i => i.MatchBr(out _));

            c.Index += 2;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate(this.RandomlyTransformDropTableForShopTerminal());
        }

        public void SceneDirector_SelectCard(ILContext il)
        {
            var c = new ILCursor(il);

            c.GotoNext(
                x => x.MatchLdsfld<SceneDirector>(nameof(SceneDirector.cardSelector)),
                x => x.MatchLdloc(2),
                x => x.MatchCallvirt("WeightedSelection`1<RoR2.DirectorCard>", nameof(WeightedSelection<DirectorCard>.AddChoice)));

            c.Index += 2;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<WeightedSelection<DirectorCard>.ChoiceInfo, SceneDirector, WeightedSelection<DirectorCard>.ChoiceInfo>>(this.TransformChoice);
        }

        private static PlayerCharacterMasterController? GetAttackingPlayerFromDamageReportInternal(DamageReport damageReport)
        {
            return damageReport.attackerMaster?.playerCharacterMasterController ?? damageReport.attackerOwnerMaster?.playerCharacterMasterController;
        }

        private static float GetEffectiveMaxHealthForFallDamage(HealthComponent healthComponent)
        {
            // Deal fall damage as % of health + shield instead of just health
            var output = healthComponent.fullCombinedHealth;

            if (healthComponent.shield > 0f)
            {
                // Slight reduction if the character has any shield
                output /= 1.04f;
            }

            return output;
        }

        private Func<DamageReport, PlayerCharacterMasterController?> GetAttackingPlayerFromDamageReport([CallerMemberName] string? context = null) => damageReport =>
        {
            this.logger.LogDebug($"Getting attacking player from damage report in context {context}");
            return GetAttackingPlayerFromDamageReportInternal(damageReport);
        };

        private PickupIndex[] RandomlyTransformPickupIndexArray(PickupIndex[] array, CharacterBody owner)
        {
            _ = this.TryRandomlyTransformPickupIndexArray(ref array, owner);
            return array;
        }

        private bool TryRandomlyTransformPickupIndexArray(ref PickupIndex[] array, CharacterBody owner)
        {
            if (!owner)
            {
                this.logger.LogWarning("Owner character body is null in RandomlyTransformPickupIndexArray - consider getting master component instead, maybe through interactor.");
                return false;
            }

            if (!this.ShouldTryRollToSeeIfShouldStack())
            {
                return false;
            }

            Dictionary<PickupIndex, (bool shouldStackRarity, bool isOwned)> dictionary = array.ToDictionary(x => x, pickupIndex =>
            {
                var pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
                return (PickupDropTableUtils.ShouldTryStackRarity(PickupDropTableUtils.GetRarity(pickupDef)), this.GetFilter(owner.master?.playerCharacterMasterController)(pickupDef));
            });

            PickupIndex[] filtered;

            bool tryPickUnownedItem = Run.instance.treasureRng.nextNormalizedFloat < GetUnownedItemProbability(dictionary.Count(x => x.Value.isOwned), dictionary.Count);

            // TODO: DRY
            if (tryPickUnownedItem)
            {
                filtered = array.Where(pickupIndex =>
                {
                    var (shouldStackRarity, isOwned) = dictionary[pickupIndex];
                    return !shouldStackRarity || !isOwned;
                }).ToArray();
            }
            else
            {
                filtered = array.Where(pickupIndex =>
                {
                    var (shouldStackRarity, isOwned) = dictionary[pickupIndex];
                    return !shouldStackRarity || isOwned;
                }).ToArray();
            }

            if (filtered.Length > 0)
            {
                array = filtered;
                return true;
            }

            return false;
        }

        private WeightedSelection<DirectorCard>.ChoiceInfo TransformChoice(WeightedSelection<DirectorCard>.ChoiceInfo choice, SceneDirector sceneDirector)
        {
            if (this.waitingForScrapper.Contains(sceneDirector) && IsScrapper(choice.value))
            {
                return new WeightedSelection<DirectorCard>.ChoiceInfo { value = choice.value, weight = choice.weight * 2 };
            }
            else
            {
                return choice;
            }
        }
    }
}
using EntityStates.FalseSonBoss;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.Artifacts;
using System;

namespace AssortedExperiments.ItemBias
{
    public partial class ItemBias : Module
    {
        public override void Init()
        {
            this.filterFactory = new FilterFactory(this.Logger, this.Settings);

            // Shops, printers etc.
            IL.RoR2.ShopTerminalBehavior.GenerateNewPickupServer_bool += this.ShopTerminalBehavior_GenerateNewPickupServer_bool;

            // Chests
            IL.RoR2.ChestBehavior.Roll += this.ChestBehavior_Roll;

            // Teleporter Boss
            IL.RoR2.BossGroup.DropRewards += this.BossGroup_DropRewards;

            // Halcyonite Shrine / Crack all geodes
            On.RoR2.PickupPickerController.GenerateOptionsFromDropTablePlusForcedStorm += this.PickupPickerController_GenerateOptionsFromDropTablePlusForcedStorm;

            // Recycler
            IL.RoR2.EquipmentSlot.FireRecycle += this.EquipmentSlot_FireRecycle;

            // Sonorous Whispers
            IL.RoR2.GlobalEventManager.OnCharacterDeath += this.GlobalEventManager_OnCharacterDeath;

            // Artifact of Sacrifice
            IL.RoR2.Artifacts.SacrificeArtifactManager.OnServerCharacterDeath += this.SacrificeArtifactManager_OnServerCharacterDeath;

            // Shrine of Chance
            IL.RoR2.ShrineChanceBehavior.AddShrineStack += this.ShrineChanceBehavior_AddShrineStack;

            // Void field cell rewards
            IL.RoR2.ArenaMissionController.EndRound += this.ArenaMissionController_EndRound;

            // Simulacrum wave rewards
            IL.RoR2.InfiniteTowerWaveController.DropRewards += this.InfiniteTowerWaveController_DropRewards;

            // Void potential chest
            IL.RoR2.OptionChestBehavior.Roll += this.OptionChestBehavior_Roll;

            // Adaptive Chest
            IL.RoR2.RouletteChestController.GenerateEntriesServer += this.RouletteChestController_GenerateEntriesServer;

            // False Son loot
            IL.EntityStates.FalseSonBoss.SkyJumpDeathState.GiveColossusItem += this.SkyJumpDeathState_GiveColossusItem;

            // TODO: For each hook, review the point at which rewards are locked in, and consider optimizing it.
            // Maybe add one or more config settings, e.g. is chest loot rerolled upon opening, and
            // are void potential orb options rolled for each player that opens the UI...
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
                x => x.MatchSet<ChestBehavior>(nameof(ChestBehavior.currentPickup)),
                x => x.MatchRet());

            c.Index += 3;
            c.EmitDelegate(this.RandomlyTransformDropTable());
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

        public PickupPickerController.Option[] PickupPickerController_GenerateOptionsFromDropTablePlusForcedStorm(On.RoR2.PickupPickerController.orig_GenerateOptionsFromDropTablePlusForcedStorm orig, int numOptions, PickupDropTable dropTable, PickupDropTable stormDropTable, Xoroshiro128Plus rng)
        {
            return orig(numOptions, this.RandomlyTransformDropTable()(dropTable), this.RandomlyTransformDropTable()(stormDropTable), rng);
        }

        public void EquipmentSlot_FireRecycle(ILContext il)
        {
            var c = new ILCursor(il);

            c.GotoNext(
                x => x.MatchLdloc(2),
                x => x.MatchCallvirt<Xoroshiro128Plus>(nameof(Xoroshiro128Plus.NextElementUniform)));

            c.Index += 1;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitGet<EquipmentSlot>(nameof(EquipmentSlot.characterBody));
            c.EmitDelegate<Func<PickupIndex[], CharacterBody, PickupIndex[]>>(this.RandomlyTransformPickupIndexArray);
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

        public void OptionChestBehavior_Roll(ILContext il)
        {
            // TODO: make this depend on the interactor, or delay the shenanigans until someone actually opens the orb
            var c = new ILCursor(il);

            c.GotoNext(x => x.MatchLdfld<OptionChestBehavior>(nameof(OptionChestBehavior.dropTable)));
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
    }
}
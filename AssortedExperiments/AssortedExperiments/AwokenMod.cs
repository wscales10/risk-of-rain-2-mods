namespace AssortedExperiments
{
    internal class AwokenMod
    {
        public AwokenMod(OnHooks on, ILHooks il)
        {
            On.RoR2.SceneDirector.PopulateScene += on.SceneDirector_PopulateScene;
            On.EntityStates.ScavBackpack.Opening.OnEnter += OnHooks.Opening_OnEnter;
            On.RoR2.SceneCatalog.OnActiveSceneChanged += on.SceneCatalog_OnActiveSceneChanged;
            On.RoR2.TeleporterInteraction.ChargedState.OnEnter += on.ChargedState_OnEnter;
            On.RoR2.SceneDirector.GenerateInteractableCardSelection += on.SceneDirector_GenerateInteractableCardSelection;
            IL.RoR2.SceneDirector.SelectCard += il.SceneDirector_SelectCard;
            On.RoR2.SceneDirector.SelectCard += on.SceneDirector_SelectCard;

            // Don't regenerate scrap at the start of special environments.
            On.RoR2.CharacterMaster.TryRegenerateScrap += on.CharacterMaster_TryRegenerateScrap;

            // Shops, printers etc.
            IL.RoR2.ShopTerminalBehavior.GenerateNewPickupServer_bool += il.ShopTerminalBehavior_GenerateNewPickupServer_bool;

            // Chests
            IL.RoR2.ChestBehavior.Roll += il.ChestBehavior_Roll;

            // Teleporter Boss
            IL.RoR2.BossGroup.DropRewards += il.BossGroup_DropRewards;

            // Halcyonite Shrine / Crack all geodes
            On.RoR2.PickupPickerController.GenerateOptionsFromDropTablePlusForcedStorm += on.PickupPickerController_GenerateOptionsFromDropTablePlusForcedStorm;

            // Recycler
            IL.RoR2.EquipmentSlot.FireRecycle += il.EquipmentSlot_FireRecycle;

            // Sonorous Whispers
            IL.RoR2.GlobalEventManager.OnCharacterDeath += il.GlobalEventManager_OnCharacterDeath;

            // Artifact of Sacrifice
            IL.RoR2.Artifacts.SacrificeArtifactManager.OnServerCharacterDeath += il.SacrificeArtifactManager_OnServerCharacterDeath;

            // Shrine of Chance
            IL.RoR2.ShrineChanceBehavior.AddShrineStack += il.ShrineChanceBehavior_AddShrineStack;

            // Void field cell rewards
            IL.RoR2.ArenaMissionController.EndRound += il.ArenaMissionController_EndRound;

            // Simulacrum wave rewards
            IL.RoR2.InfiniteTowerWaveController.DropRewards += il.InfiniteTowerWaveController_DropRewards;

            // Void potential chest
            IL.RoR2.OptionChestBehavior.Roll += il.OptionChestBehavior_Roll;

            // Adaptive Chest
            IL.RoR2.RouletteChestController.GenerateEntriesServer += il.RouletteChestController_GenerateEntriesServer;

            // False Son loot
            IL.EntityStates.FalseSonBoss.SkyJumpDeathState.GiveColossusItem += il.SkyJumpDeathState_GiveColossusItem;

            // TODO: For each hook, review the point at which rewards are locked in, and consider optimizing it.
            // Maybe add one or more config settings, e.g. is chest loot rerolled upon opening, and
            // are void potential orb options rolled for each player that opens the UI...
        }
    }
}
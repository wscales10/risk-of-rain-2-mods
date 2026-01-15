using BepInEx.Configuration;

namespace AssortedExperiments
{
    internal class Settings
    {
        private readonly ConfigEntry<float> stage1interactableCreditMultiplier;

        private readonly ConfigEntry<bool> testMode;

        private readonly ConfigEntry<int> preferredMaxStacks;

        private readonly ConfigEntry<bool> scrapOnlyRegeneratesOnNormalStages;

        private readonly ConfigEntry<float> voidInteractableSpawnChanceMultiplier;

        private readonly ConfigEntry<bool> preventHalcyoniteShrinesOnStage3Environments;

        private readonly ConfigEntry<float> adjustmentStrengthFor3dPrinters;

        public Settings(ConfigFile config)
        {
            this.stage1interactableCreditMultiplier = config.Bind("Stage Features", "Stage 1 Interactable Credit Multiplier", 0.75f, "Multiplier for interactable credits on stage 1 when the Sacrifice Artifact is not enabled.");
            this.testMode = config.Bind("General", "Test Mode", false, "Instead of preferring already owned pickups, prefer damage items and low cooldown equipment.");
            this.preferredMaxStacks = config.Bind("Item Bias", "Preferred Maximum Stack Count", 10, "Only prefer owned items when the player's stack count for that item is below this value. Set to 0 to disable.");
            this.scrapOnlyRegeneratesOnNormalStages = config.Bind("General", "Only Regenerate Scrap on Normal Stages", true, "Only regenerate scrap on normal stages (including the moon), not special environments like the Bazaar.");
            this.voidInteractableSpawnChanceMultiplier = config.Bind("Stage Features", "Void Interactable Spawn Chance Multiplier", 0.6f, "Multiplier modifying how common void cradles and void seeds are.");
            this.preventHalcyoniteShrinesOnStage3Environments = config.Bind("Stage Features", "Prevent Halcyonite Shrines on Stage 3 Environments", true, "Sue me, I keep accidentally going to Prime Meridian.");
            this.adjustmentStrengthFor3dPrinters = config.Bind("Item Bias", "Adjustment Strength for 3D Printers", 0.4f, "How closely 3D printers should stick to the mod's standard model of item weight adjustment. 0 = no adjustment (similar to vanilla), 1 = standard adjustment.");
        }

        public float Stage1InteractableCreditMultiplier => this.stage1interactableCreditMultiplier.Value;

        public bool TestMode => this.testMode.Value;

        public int PreferredMaxStacks => this.preferredMaxStacks.Value;

        public bool ScrapOnlyRegeneratesOnNormalStages => this.scrapOnlyRegeneratesOnNormalStages.Value;

        public float VoidInteractableSpawnChanceMultiplier => this.voidInteractableSpawnChanceMultiplier.Value;

        public bool PreventHalcyoniteShrinesOnStage3Environments => this.preventHalcyoniteShrinesOnStage3Environments.Value;

        public float AdjustmentStrengthFor3dPrinters => this.adjustmentStrengthFor3dPrinters.Value;
    }
}
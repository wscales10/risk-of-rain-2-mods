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

        public Settings(ConfigFile config)
        {
            this.stage1interactableCreditMultiplier = config.Bind("General", "Stage 1 Interactable Credit Multiplier", 0.75f, "Multiplier for interactable credits on stage 1 when the Sacrifice Artifact is not enabled.");
            this.testMode = config.Bind("General", "Test Mode", false, "Instead of preferring already owned pickups, prefer damage items and low cooldown equipment.");
            this.preferredMaxStacks = config.Bind("General", "Preferred Maximum Stack Count", 10, "Only prefer owned items when the player's stack count for that item is below this value. Set to 0 to disable.");
            this.scrapOnlyRegeneratesOnNormalStages = config.Bind("General", "Only Regenerate Scrap on Normal Stages", true, "Only regenerate scrap on normal stages (including the moon), not special environments like the Bazaar.");
            this.voidInteractableSpawnChanceMultiplier = config.Bind("General", "Void Interactable Spawn Chance Multiplier", 0.7f, "Multiplier modifying how common void cradles and void seeds are.");
            this.preventHalcyoniteShrinesOnStage3Environments = config.Bind("General", "Prevent Halcyonite Shrines on Stage 3 Environments", true, "Sue me, I keep accidentally going to Prime Meridian.");
        }

        public float Stage1InteractableCreditMultiplier => this.stage1interactableCreditMultiplier.Value;

        public bool TestMode => this.testMode.Value;

        public int PreferredMaxStacks => this.preferredMaxStacks.Value;

        public bool ScrapOnlyRegeneratesOnNormalStages => this.scrapOnlyRegeneratesOnNormalStages.Value;

        public float VoidInteractableSpawnChanceMultiplier => this.voidInteractableSpawnChanceMultiplier.Value;

        public bool PreventHalcyoniteShrinesOnStage3Environments => this.preventHalcyoniteShrinesOnStage3Environments.Value;
    }
}
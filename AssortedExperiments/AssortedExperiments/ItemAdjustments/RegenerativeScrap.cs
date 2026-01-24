using RoR2;

namespace AssortedExperiments.ItemAdjustments
{
    public class RegenerativeScrap : Module
    {
        public override void Init()
        {
            // Don't regenerate scrap at the start of special environments.
            On.RoR2.CharacterMaster.TryRegenerateScrap += this.CharacterMaster_TryRegenerateScrap;
        }

        private void CharacterMaster_TryRegenerateScrap(On.RoR2.CharacterMaster.orig_TryRegenerateScrap orig, CharacterMaster self)
        {
            if (this.Settings.ScrapOnlyRegeneratesOnNormalStages && SceneCatalog.currentSceneDef?.sceneType != SceneType.Stage)
            {
                return;
            }

            orig(self);
        }
    }
}
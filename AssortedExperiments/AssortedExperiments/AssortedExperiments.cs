using BepInEx;
using BepInEx.Configuration;
using RoR2;
using System;

namespace AssortedExperiments
{
    [BepInPlugin("com.woodyscales.assortedexperiments", "Assorted Experiments", "1.0.0")]
    public class AssortedExperiments : BaseUnityPlugin
    {
        private ConfigEntry<float> interactableCreditMultiplier;

        public void Awake()
        {
            this.interactableCreditMultiplier = this.Config.Bind("General", "Interactable Credit Multiplier", 0.75f, "Multiplier for interactable credits when the Sacrifice Artifact is not enabled.");
            On.RoR2.SceneDirector.PopulateScene += this.SceneDirector_PopulateScene;
            On.EntityStates.ScavBackpack.Opening.OnEnter += Opening_OnEnter;
        }

        private static void Opening_OnEnter(On.EntityStates.ScavBackpack.Opening.orig_OnEnter orig, EntityStates.ScavBackpack.Opening self)
        {
            EntityStates.ScavBackpack.Opening.maxItemDropCount = 9;
            orig(self);
        }

        private void SceneDirector_PopulateScene(On.RoR2.SceneDirector.orig_PopulateScene orig, SceneDirector self)
        {
            if (RunArtifactManager.instance?.IsArtifactEnabled(RoR2Content.Artifacts.sacrificeArtifactDef) != true)
            {
                var multiplier = this.interactableCreditMultiplier.Value;
                this.Logger.LogInfo($"Applying interactable credit reduction of {Math.Round(100 * (1 - multiplier))}%.");
                self.onPopulateCreditMultiplier *= multiplier;
            }

            orig(self);
        }
    }
}
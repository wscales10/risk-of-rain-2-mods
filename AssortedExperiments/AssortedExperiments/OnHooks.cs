using BepInEx.Logging;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace AssortedExperiments
{
    internal class OnHooks : Hooks
    {
        public OnHooks(ManualLogSource logger, Settings settings, HashSet<SceneDirector> waitingForScrapper) : base(logger, settings, waitingForScrapper)
        {
        }

        public static void Opening_OnEnter(On.EntityStates.ScavBackpack.Opening.orig_OnEnter orig, EntityStates.ScavBackpack.Opening self)
        {
            EntityStates.ScavBackpack.Opening.maxItemDropCount = 9;
            orig(self);
        }

        public WeightedSelection<DirectorCard> SceneDirector_GenerateInteractableCardSelection(On.RoR2.SceneDirector.orig_GenerateInteractableCardSelection orig, SceneDirector self)
        {
            var originalResult = orig(self);
            var output = new WeightedSelection<DirectorCard>(originalResult.Capacity);
            bool preventHalcyoniteShrinesOnThisStage = Run.instance?.stageClearCountInCurrentLoop == 2 && this.settings.PreventHalcyoniteShrinesOnStage3Environments;

            foreach (var choice in originalResult.choices)
            {
                string? spawnCardName = choice.value?.spawnCard?.name;

                if (spawnCardName is null)
                {
                    continue;
                }

                if (preventHalcyoniteShrinesOnThisStage && spawnCardName.Contains("ShrineHalcyonite"))
                {
                    continue;
                }

                if (spawnCardName.Contains("VoidChest") || spawnCardName.Contains("VoidCamp"))
                {
                    output.AddChoice(choice.value!, choice.weight * this.settings.VoidInteractableSpawnChanceMultiplier);
                }
                else
                {
                    output.AddChoice(choice);
                }
            }

            return output;
        }

        public void ChargedState_OnEnter(On.RoR2.TeleporterInteraction.ChargedState.orig_OnEnter orig, TeleporterInteraction.ChargedState self)
        {
            orig(self);
            this.logger.LogInfo($"Teleporter charged - {GetTimeString()}");
        }

        public void SceneCatalog_OnActiveSceneChanged(On.RoR2.SceneCatalog.orig_OnActiveSceneChanged orig, Scene oldScene, Scene newScene)
        {
            orig(oldScene, newScene);
            this.logger.LogInfo($"Scene changed from '{GetSceneDisplayName(oldScene)}' to '{GetSceneDisplayName(newScene)}'- {GetTimeString()}");
        }

        public void CharacterMaster_TryRegenerateScrap(On.RoR2.CharacterMaster.orig_TryRegenerateScrap orig, CharacterMaster self)
        {
            if (this.settings.ScrapOnlyRegeneratesOnNormalStages && SceneCatalog.currentSceneDef?.sceneType != SceneType.Stage)
            {
                return;
            }

            orig(self);
        }

        public PickupPickerController.Option[] PickupPickerController_GenerateOptionsFromDropTablePlusForcedStorm(On.RoR2.PickupPickerController.orig_GenerateOptionsFromDropTablePlusForcedStorm orig, int numOptions, PickupDropTable dropTable, PickupDropTable stormDropTable, Xoroshiro128Plus rng)
        {
            return orig(numOptions, this.RandomlyTransformDropTable()(dropTable), this.RandomlyTransformDropTable()(stormDropTable), rng);
        }

        public void SceneDirector_PopulateScene(On.RoR2.SceneDirector.orig_PopulateScene orig, SceneDirector self)
        {
            if (!this.settings.TestMode && (Run.instance?.stageClearCount == 0 || Run.instance?.stageClearCount > 9) && RunArtifactManager.instance?.IsArtifactEnabled(RoR2Content.Artifacts.sacrificeArtifactDef) != true)
            {
                var multiplier = this.settings.Stage1InteractableCreditMultiplier;
                this.logger.LogInfo($"Applying interactable credit reduction of {Math.Round(100 * (1 - multiplier))}%.");
                self.onPopulateCreditMultiplier *= multiplier;
            }

            this.waitingForScrapper.Add(self);

            try
            {
                orig(self);
            }
            finally
            {
                this.waitingForScrapper.Remove(self);
            }
        }

        public DirectorCard SceneDirector_SelectCard(On.RoR2.SceneDirector.orig_SelectCard orig, SceneDirector self, WeightedSelection<DirectorCard> deck, int maxCost)
        {
            var output = orig(self, deck, maxCost);

            if (IsScrapper(output))
            {
                this.waitingForScrapper.Remove(self);
            }

            return output;
        }

        private static string GetTimeString()
        {
            var run = Run.instance;

            if (run is null)
            {
                return "no run";
            }

            return $"Stage {run.stageClearCount + 1} / {TimeSpan.FromSeconds(run.GetRunStopwatch()):mm\\:ss\\.ff} / coef {run.difficultyCoefficient}";
        }

        private static string GetSceneDisplayName(Scene scene)
        {
            try
            {
                SceneDef sceneDef = SceneCatalog.GetSceneDefFromScene(scene);
                return Language.GetString(sceneDef.nameToken);
            }
            catch
            {
                return "??";
            }
        }
    }
}
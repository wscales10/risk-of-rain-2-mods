using BepInEx.Logging;
using HG;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
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

        public static void BarrageOnBossBehaviour_OnDisable(On.RoR2.BarrageOnBossBehaviour.orig_OnDisable orig, BarrageOnBossBehaviour self)
        {
            orig(self);

            if (self.TryGetComponent<BossBarrageContext>(out var context))
            {
                context.Position = null;
            }
        }

        public static void BarrageOnBossBehaviour_UpdateBarrage(On.RoR2.BarrageOnBossBehaviour.orig_UpdateBarrage orig, BarrageOnBossBehaviour self)
        {
            orig(self);

            if (self.bossmissileState == BarrageOnBossBehaviour.BossMissileState.None && self.TryGetComponent<BossBarrageContext>(out var context))
            {
                context.Position = null;
            }
        }

        public Vector3 BarrageOnBossBehaviour_CalculateHitPosition(On.RoR2.BarrageOnBossBehaviour.orig_CalculateHitPosition orig, BarrageOnBossBehaviour self, GameObject target)
        {
            (Vector3, float) ResolveSpreadOriginAndRadius()
            {
                var context = self.EnsureComponent<BossBarrageContext>();

                if (!target)
                {
                    if (context && context.Position.HasValue)
                    {
                        return (context.Position.Value, context.SpreadRadius);
                    }
                    else
                    {
                        target = self.gameObject;
                    }
                }

                var spreadOrigin = self.MoveTargetToGround(target.transform.position);

                this.logger.LogDebug($"Setting War Bonds origin position to {spreadOrigin}");
                context.Position = spreadOrigin;

                float spreadRadius;
                if (self.isTargetBoss)
                {
                    spreadRadius = 1f;
                }
                else if (target == self.gameObject)
                {
                    spreadRadius = 10f;
                }
                else
                {
                    spreadRadius = 0f;
                }

                this.logger.LogDebug($"Setting War Bonds spread radius to {spreadRadius}");
                context.SpreadRadius = spreadRadius;
                return (spreadOrigin, spreadRadius);
            }

            var (spreadOrigin, spreadRadius) = ResolveSpreadOriginAndRadius();
            Vector2 normalized = UnityEngine.Random.insideUnitCircle.normalized;
            return spreadOrigin + new Vector3(normalized.x * self.BarrageRadius * spreadRadius, 0f, normalized.y * self.BarrageRadius * spreadRadius);
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

        public void ZiplineVehicle_OnPassengerEnter(On.RoR2.ZiplineVehicle.orig_OnPassengerEnter orig, ZiplineVehicle self, GameObject passenger)
        {
            orig(self, passenger);
            var characterBody = passenger.GetComponent<CharacterBody>();

            if (!characterBody) return;

            var multiplier = characterBody.moveSpeed / 7f;
            this.logger.LogInfo($"Multiplying eccentric vase acceleration and max speed by {multiplier}");

            // TODO: these numbers 30 and 10 are copied from the ZiplineVehicle prefab; this is not great, as it will override any changes made to this prefab or by other mods
            self.acceleration = 30f * multiplier;
            self.maxSpeed = 10f * multiplier;
        }
    }
}
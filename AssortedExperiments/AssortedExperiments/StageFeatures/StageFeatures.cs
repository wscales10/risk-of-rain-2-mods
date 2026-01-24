using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using System.Collections.Generic;

namespace AssortedExperiments.StageFeatures
{
    public class StageFeatures : Module
    {
        private readonly HashSet<SceneDirector> waitingForScrapper = new HashSet<SceneDirector>();

        public override void Init()
        {
            this.waitingForScrapper.Clear();
            On.RoR2.SceneDirector.GenerateInteractableCardSelection += this.SceneDirector_GenerateInteractableCardSelection;
            On.RoR2.SceneDirector.SelectCard += this.SceneDirector_SelectCard;
            IL.RoR2.SceneDirector.SelectCard += this.SceneDirector_SelectCard;
            On.RoR2.SceneDirector.PopulateScene += this.SceneDirector_PopulateScene;

            On.EntityStates.ScavBackpack.Opening.OnEnter += Opening_OnEnter;
        }

        public void SceneDirector_PopulateScene(On.RoR2.SceneDirector.orig_PopulateScene orig, SceneDirector self)
        {
            if (!this.Settings.TestMode && (Run.instance?.stageClearCount == 0 || Run.instance?.stageClearCount > 9) && RunArtifactManager.instance?.IsArtifactEnabled(RoR2Content.Artifacts.sacrificeArtifactDef) != true)
            {
                var multiplier = this.Settings.Stage1InteractableCreditMultiplier;
                this.Logger.LogInfo($"Applying interactable credit reduction of {Math.Round(100 * (1 - multiplier))}%.");
                self.onPopulateCreditMultiplier *= multiplier;
            }

            if (RunArtifactManager.instance?.IsArtifactEnabled(CU8Content.Artifacts.Devotion) != true)
            {
                this.waitingForScrapper.Add(self);
            }

            try
            {
                orig(self);
            }
            finally
            {
                this.waitingForScrapper.Remove(self);
            }
        }

        private static void Opening_OnEnter(On.EntityStates.ScavBackpack.Opening.orig_OnEnter orig, EntityStates.ScavBackpack.Opening self)
        {
            EntityStates.ScavBackpack.Opening.maxItemDropCount = 9;
            orig(self);
        }

        private WeightedSelection<DirectorCard> SceneDirector_GenerateInteractableCardSelection(On.RoR2.SceneDirector.orig_GenerateInteractableCardSelection orig, SceneDirector self)
        {
            var originalResult = orig(self);
            var output = new WeightedSelection<DirectorCard>(originalResult.Capacity);
            bool preventHalcyoniteShrinesOnThisStage = Run.instance?.stageClearCountInCurrentLoop == 2 && this.Settings.PreventHalcyoniteShrinesOnStage3Environments;

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
                    output.AddChoice(choice.value!, choice.weight * this.Settings.VoidInteractableSpawnChanceMultiplier);
                }
                else
                {
                    output.AddChoice(choice);
                }
            }

            return output;
        }

        private void SceneDirector_SelectCard(ILContext il)
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

        private DirectorCard SceneDirector_SelectCard(On.RoR2.SceneDirector.orig_SelectCard orig, SceneDirector self, WeightedSelection<DirectorCard> deck, int maxCost)
        {
            var output = orig(self, deck, maxCost);

            if (Utils.IsScrapper(output))
            {
                this.waitingForScrapper.Remove(self);
            }

            return output;
        }

        private WeightedSelection<DirectorCard>.ChoiceInfo TransformChoice(WeightedSelection<DirectorCard>.ChoiceInfo choice, SceneDirector sceneDirector)
        {
            if (this.waitingForScrapper.Contains(sceneDirector) && Utils.IsScrapper(choice.value))
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
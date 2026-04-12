using HG;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PactOfPunishment.Conditions
{
    public sealed class StageProgressionCost : DefaultConditionDef
    {
        private const int itemsToScrapPerRank = 2;

        private AssetPromise<InteractableSpawnCard> scrapperCard;

        public override int MaxRank => 1;

        public override int HeatPerRank => 2;

        public override string Description => string.Format(base.Description, itemsToScrapPerRank);

        public override void Init()
        {
            IL.RoR2.InfiniteTowerRun.OnWaveAllEnemiesDefeatedServer += this.InfiniteTowerRun_OnWaveAllEnemiesDefeatedServer1;
            On.RoR2.ScrapperController.CreateItemTakenOrb += ScrapperController_CreateItemTakenOrb;
            On.RoR2.InfiniteTowerRun.Start += this.InfiniteTowerRun_Start;
        }

        private static void ScrapperController_CreateItemTakenOrb(On.RoR2.ScrapperController.orig_CreateItemTakenOrb orig, Vector3 effectOrigin, GameObject targetObject, ItemIndex itemIndex)
        {
            orig(effectOrigin, targetObject, itemIndex);

            if (Run.instance.TryGetComponent<UnderworldCustomsBehavior>(out var behavior))
            {
                behavior.OnItemScrapped();
            }
        }

        private void InfiniteTowerRun_Start(On.RoR2.InfiniteTowerRun.orig_Start orig, InfiniteTowerRun self)
        {
            this.scrapperCard = Utils.BeginLoad<InteractableSpawnCard>("RoR2/Base/Scrapper/iscScrapper.asset", this.Logger);
            orig(self);
        }

        private void InfiniteTowerRun_OnWaveAllEnemiesDefeatedServer1(ILContext il)
        {
            var c = new ILCursor(il);

            c.Index = c.Instrs.Count - 1;
            c.GotoPrev(x => x.MatchCallvirt<DirectorCore>(nameof(DirectorCore.TrySpawnObject)), x => x.MatchPop()); // TODO: more robust check for portal spawning
            c.Index++;
            c.Remove();
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<GameObject, InfiniteTowerRun>>((portalObject, self) =>
            {
                // TODO: don't spawn scrapper if there is already one near the safe ward
                DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(this.scrapperCard.Value, new DirectorPlacementRule
                {
                    minDistance = 0f,
                    maxDistance = self.stageTransitionPortalMaxDistance,
                    placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                    position = self.safeWardController.transform.position,
                    spawnOnTarget = self.safeWardController.transform
                }, self.safeWardRng));

                this.TrySpawnGreenPortal(self);

                if (!this.IsEnabled(self))
                {
                    return;
                }

                if (!portalObject.GetComponent<SceneExitController>())
                {
                    throw new InvalidOperationException("Not a portal");
                }

                // TODO: disable "activate portal" objective while portal not enabled
                portalObject.GetComponent<GenericInteraction>().SetInteractabilityConditionsNotMet();

                var behavior = Run.instance.EnsureComponent<UnderworldCustomsBehavior>();
                behavior.OpenShop(this.GetRank(self) * itemsToScrapPerRank);
                behavior.onEnoughItemsScrapped = portalObject.GetComponent<GenericInteraction>().SetInteractabilityAvailable;
            });
        }

        private void TrySpawnGreenPortal(InfiniteTowerRun run)
        {
            if (run.TryGetComponent<PrimeMeridian.GreenPortalSpawnerBehavior>(out var behavior) && behavior.GreenPortalSpawner is PortalSpawner ps) // TODO: move to other class.
            {
                ps.spawnReferenceLocationOverride = run.safeWardController.transform;

                if (!ps.AttemptSpawnPortalServer())
                {
                    this.Logger.LogWarning("Failed to spawn green portal");
                }
            }
        }

        public class UnderworldCustomsBehavior : MonoBehaviour
        {
            public Action? onEnoughItemsScrapped;

            private int itemsToScrap;

            public int ItemsToScrap
            {
                get => this.itemsToScrap;

                private set
                {
                    this.itemsToScrap = value;

                    if (value > 0)
                    {
                        ObjectivePanelController.collectObjectiveSources += this.ObjectivePanelController_collectObjectiveSources;
                    }
                    else
                    {
                        ObjectivePanelController.collectObjectiveSources -= this.ObjectivePanelController_collectObjectiveSources;
                    }
                }
            }

            public void OpenShop(int numberOfItemsToScrap)
            {
                this.ItemsToScrap = this.TotalItemsToScrap = numberOfItemsToScrap;
            }

            public int TotalItemsToScrap { get; private set; }

            public void OnItemScrapped()
            {
                if (this.ItemsToScrap > 0)
                {
                    this.ItemsToScrap--;

                    if (this.ItemsToScrap < 1)
                    {
                        this.ItemsToScrap = 0;
                        var callback = this.onEnoughItemsScrapped;
                        this.onEnoughItemsScrapped = null;
                        callback?.Invoke();
                    }
                }
            }

            private void ObjectivePanelController_collectObjectiveSources(CharacterMaster master, List<ObjectivePanelController.ObjectiveSourceDescriptor> output)
            {
                output.Add(new ObjectivePanelController.ObjectiveSourceDescriptor
                {
                    source = this,
                    master = master,
                    objectiveType = typeof(ObjectiveTracker)
                });
            }

            private sealed class ObjectiveTracker : ObjectivePanelController.ObjectiveTracker
            {
                private UnderworldCustomsBehavior Source => (UnderworldCustomsBehavior)this.sourceDescriptor.source;

                public ObjectiveTracker()
                {
                    this.baseToken = "SCRAPPER_CONTEXT";
                }

                public override string GenerateString()
                {
                    return Language.GetStringFormatted("OBJECTIVE_FRACTION_PROGRESS_FORMAT", base.GenerateString(), this.Source.TotalItemsToScrap - this.Source.ItemsToScrap, this.Source.TotalItemsToScrap);
                }
            }
        }
    }
}
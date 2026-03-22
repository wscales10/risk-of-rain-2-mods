using HG;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using UnityEngine;

namespace PactOfPunishment.Conditions
{
    public sealed class UnderworldCustoms : DefaultConditionDef
    {
        private AssetPromise<InteractableSpawnCard> scrapperCard;

        public override int MaxRank => 1;

        public override int HeatPerRank => 2;

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

                portalObject.GetComponent<GenericInteraction>().SetInteractabilityConditionsNotMet();

                var behavior = Run.instance.EnsureComponent<UnderworldCustomsBehavior>();
                behavior.itemsToScrap = this.GetRank(self);
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
            public int itemsToScrap;

            public Action? onEnoughItemsScrapped;

            public void OnItemScrapped()
            {
                if (this.itemsToScrap > 0)
                {
                    this.itemsToScrap--;

                    if (this.itemsToScrap < 1)
                    {
                        this.itemsToScrap = 0;
                        var callback = this.onEnoughItemsScrapped;
                        this.onEnoughItemsScrapped = null;
                        callback?.Invoke();
                    }
                }
            }
        }
    }
}
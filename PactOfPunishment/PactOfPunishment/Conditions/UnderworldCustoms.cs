using HG;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using PactOfPunishment.Waves.Infrastructure;
using RoR2;
using System;
using UnityEngine;

namespace PactOfPunishment.Conditions
{
    public sealed class UnderworldCustoms : DefaultConditionDef
    {
        private AssetPromise<InteractableSpawnCard> scrapperCard;

        private AssetPromise<InteractableSpawnCard> greenPortalCard;

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
            this.greenPortalCard = Utils.BeginLoad<InteractableSpawnCard>("RoR2/DLC2/iscColossusPortal.asset", this.Logger);
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
                if (self.TryGetComponent<SimulacrumWavesBehavior>(out var simulacrumWavesBehavior)) // TODO: move to other class and disable fogDamageController on prime meridian. For now, instead of spawning a green portal after the false son boss fight (make this driven by a bool somewhere), just allow the player to offer up an item to end the run.
                {
                    simulacrumWavesBehavior.GreenPortalSpawner.spawnReferenceLocationOverride = self.safeWardController.transform;
                    simulacrumWavesBehavior.GreenPortalSpawner.AttemptSpawnPortalServer();
                }

                if (!this.IsEnabled(self))
                {
                    return;
                }

                if (!portalObject.GetComponent<SceneExitController>())
                {
                    throw new InvalidOperationException("Not a portal");
                }

                portalObject.GetComponent<GenericInteraction>().SetInteractabilityConditionsNotMet();

                DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(this.scrapperCard.Value, new DirectorPlacementRule
                {
                    minDistance = 0f,
                    maxDistance = self.stageTransitionPortalMaxDistance,
                    placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                    position = self.safeWardController.transform.position,
                    spawnOnTarget = self.safeWardController.transform
                }, self.safeWardRng));

                var behavior = Run.instance.EnsureComponent<UnderworldCustomsBehavior>();
                behavior.itemsToScrap = this.GetRank(self);
                behavior.onEnoughItemsScrapped = portalObject.GetComponent<GenericInteraction>().SetInteractabilityAvailable;
            });
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
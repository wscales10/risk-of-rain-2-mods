using HG;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using RoR2.ExpansionManagement;
using System;
using System.Linq;
using UnityEngine;

namespace PactOfPunishment
{
    public class PrimeMeridian : Module
    {
        private const string name = "GreenPortalSpawner";

        private AssetPromise<GameObject> portalSpawnerPrefab;

        public override void Init()
        {
            On.RoR2.InfiniteTowerRun.OnPrePopulateSceneServer += this.InfiniteTowerRun_OnPrePopulateSceneServer;
            On.RoR2.InfiniteTowerRun.PerformStageCleanUp += this.InfiniteTowerRun_PerformStageCleanUp;

            // On.RoR2.MeridianEventLightningTrigger.PopulateSceneWithMonsters += this.MeridianEventLightningTrigger_PopulateSceneWithMonsters;
            // - not really in Simulacrum any more?

            IL.EntityStates.ShrineRebirth.RebirthOrPortalChoice.OnEnter += Utils.HookIL(RebirthOrPortalChoice_OnEnter);
            On.EntityStates.ShrineRebirth.RevealRebirthShriine.OnEnter += this.RevealRebirthShriine_OnEnter;
            On.EntityStates.ShrineRebirth.ShrineRebirthEntityStates.SpawnACPortal += this.ShrineRebirthEntityStates_SpawnACPortal;
            On.EntityStates.MeridianEvent.FSBFPhaseBaseState.OnMemberAddedServer += this.FSBFPhaseBaseState_OnMemberAddedServer;

            Utils.DoDuringGameLoad(this.SetPortalSpawnerPrefab);
        }

        private static void RebirthOrPortalChoice_OnEnter(ILCursor c)
        {
            c.GotoLast(MoveType.After, x => x.MatchCallvirt<DirectorCore>(nameof(DirectorCore.TrySpawnObject)), x => x.MatchStloc(out _));
            var label = c.MarkLabel();
            c.GotoPrev(MoveType.AfterLabel, x => x.MatchCall<DirectorCore>($"get_{nameof(DirectorCore.instance)}"));
            c.EmitDelegate<Func<bool>>(() => Run.instance is InfiniteTowerRun);
            c.Emit(OpCodes.Brtrue_S, label);
        }

        private static GameObject MakeGreenPortalSpawnerPrefab(InteractableSpawnCard isc)
        {
            var gameObject = PrefabAPI.CreateEmptyPrefab(name);
            var ps = gameObject.AddComponent<PortalSpawner>();
            ps.portalSpawnCard = isc;
            ps.spawnChance = 1;

            // TODO: set spawnReferenceLocation?
            ps.minSpawnDistance = 10;
            ps.maxSpawnDistance = 30;

            //ps.spawnPreviewMessageToken = "PORTAL_STORM_WILL_OPEN";
            ps.spawnMessageToken = "PORTAL_STORM_OPEN";

            //ps.modelChildLocator
            //ps.previewChildName
            ps.requiredExpansion = ExpansionCatalog.expansionDefs.Single(x => x.nameToken == "DLC2_NAME");
            ps.minStagesCleared = 3;

            //ps.bannedEventFlag

            ps.validStageTiers = Array.Empty<int>();
            ps.validStages = Array.Empty<string>();
            ps.invalidStages = Array.Empty<string>();

            return gameObject;
        }

        private void FSBFPhaseBaseState_OnMemberAddedServer(On.EntityStates.MeridianEvent.FSBFPhaseBaseState.orig_OnMemberAddedServer orig, EntityStates.MeridianEvent.FSBFPhaseBaseState self, CharacterMaster master)
        {
            orig(self, master);

            var body = master.GetBody();

            // Only affects phase 1
            if (body.Is(DLC2Content.BodyPrefabs.FalseSonBossBody))
            {
                body.ScaleMaxHealth(this, 0.85f);
            }
        }

        private void ShrineRebirthEntityStates_SpawnACPortal(On.EntityStates.ShrineRebirth.ShrineRebirthEntityStates.orig_SpawnACPortal orig, EntityStates.ShrineRebirth.ShrineRebirthEntityStates self)
        {
            if (Run.instance is InfiniteTowerRun)
            {
                return;
            }

            orig(self);
        }

        private void RevealRebirthShriine_OnEnter(On.EntityStates.ShrineRebirth.RevealRebirthShriine.orig_OnEnter orig, EntityStates.ShrineRebirth.RevealRebirthShriine self)
        {
            orig(self);

            if (Run.instance is InfiniteTowerRun && !self.isEclipse)
            {
                self._shrineController.SetEntityStateMachineToRebirthOrPortal(); // TODO: test this, and interactions with alloyed collective portal
            }
        }

        private void SetPortalSpawnerPrefab()
        {
            this.portalSpawnerPrefab = Utils.BeginLoadAndTransform<InteractableSpawnCard, GameObject>("RoR2/DLC2/iscColossusPortal.asset", MakeGreenPortalSpawnerPrefab, this.Logger);
        }

        private void MeridianEventLightningTrigger_PopulateSceneWithMonsters(On.RoR2.MeridianEventLightningTrigger.orig_PopulateSceneWithMonsters orig, MeridianEventLightningTrigger self)
        {
            orig(self);

            if (Run.instance is InfiniteTowerRun run)
            {
                this.AdvanceWave(run);
            }
        }

        private void AdvanceWave(InfiniteTowerRun run)
        {
            this.Logger.LogDebug("Advancing simulacrum wave...");
            run.AdvanceWave();
            run.RecalculateDifficultyCoefficentInternal();
        }

        private void InfiniteTowerRun_PerformStageCleanUp(On.RoR2.InfiniteTowerRun.orig_PerformStageCleanUp orig, InfiniteTowerRun self)
        {
            try
            {
                if (self.TryGetComponent<GreenPortalSpawnerBehavior>(out var behavior))
                {
                    behavior.enabled = false;
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex);
            }
            finally
            {
                orig(self);
            }
        }

        private void InfiniteTowerRun_OnPrePopulateSceneServer(On.RoR2.InfiniteTowerRun.orig_OnPrePopulateSceneServer orig, InfiniteTowerRun self, SceneDirector sceneDirector)
        {
            orig(self, sceneDirector);

            if (self.safeWardController)
            {
                var component = self.EnsureComponent<GreenPortalSpawnerBehavior>();
                component.PortalSpawnerPrefab = this.portalSpawnerPrefab.Value;
                component.enabled = true;
            }
            else
            {
                if (self.fogDamageController is FogDamageController fogDamageController)
                {
                    fogDamageController.enabled = false;
                }

                self.GetComponent<EnemyInfoPanelInventoryProvider>().enabled = false; // TODO: do we need to re-enable this at some point? Is this the correct condition for disabling this?
            }
        }

        public class GreenPortalSpawnerBehavior : MonoBehaviour
        {
            private GameObject? portalSpawnerPrefab;

            public PortalSpawner? GreenPortalSpawner { get; private set; }

            internal GameObject? PortalSpawnerPrefab
            {
                get => this.portalSpawnerPrefab;

                set
                {
                    this.portalSpawnerPrefab = value;
                    this.TrySetPortalSpawner();
                }
            }

            public void OnEnable()
            {
                this.TrySetPortalSpawner();
            }

            public void TrySetPortalSpawner()
            {
                if (this.PortalSpawnerPrefab)
                {
                    this.GreenPortalSpawner = Instantiate(this.PortalSpawnerPrefab!, this.transform).GetComponent<PortalSpawner>();
                }
            }

            public void OnDisable()
            {
                Destroy(this.GreenPortalSpawner);
            }
        }
    }
}
using EntityStates.FalseSonBoss;
using MonoMod.Cil;
using RoR2;
using System;
using UnityEngine;

namespace PactOfPunishment.Waves.FinalStage
{
    public partial class FinalBoss : Module
    {
        public static bool IsFalseSonBoss(CharacterBody? body)
        {
            return body && body!.baseNameToken == DLC2Content.BodyPrefabs.FalseSonBossBody.baseNameToken;
        }

        public override void Init()
        {
            // Base
            SpawnCard.onSpawnedServerGlobal += this.SpawnCard_onSpawnedServerGlobal;

            On.RoR2.CharacterAI.BaseAI.HasLOS_Vector3 += this.BaseAI_HasLOS_Vector3;
            On.EntityStates.MeridianEvent.FSBFPhaseBaseState.OnBossGroupDefeated += this.FSBFPhaseBaseState_OnBossGroupDefeated;

            this.InitSlams();
            this.InitLunarRain();
            this.InitCorruptedPathsDash();
            this.InitTaintedOffering();

            // Upgraded
            On.EntityStates.MeridianEvent.Phase2.OnExit += this.Phase2_OnExit;
            On.EntityStates.MeridianEvent.Phase3.OnExit += this.Phase3_OnExit;
            IL.EntityStates.Geode.GeodeShatter.SearchForPlayers += Utils.HookIL(GeodeShatter_SearchForPlayers);
            IL.EntityStates.Geode.GeodeShatter.HandleCleanseNearbyPlayers += Utils.HookIL(GeodeShatter_HandleCleanseNearbyPlayers);
            On.EntityStates.Geode.GeodeInert.OnEnter += this.GeodeInert_OnEnter;

            // Music timing
            On.EntityStates.FalseSonBoss.CrystalDeathState.PlayDeathAnimation += this.CrystalDeathState_PlayDeathAnimation;
            On.EntityStates.FalseSonBoss.BrokenCrystalDeathState.PlayDeathAnimation += this.BrokenCrystalDeathState_PlayDeathAnimation;

            // On.EntityStates.MeridianEvent.Phase3.OnEnter += this.Phase3_OnEnter;
        }

        private static void GeodeShatter_HandleCleanseNearbyPlayers(ILCursor c)
        {
            c.GotoNext(
                x => x.MatchCallvirt<CharacterBody>($"get_{nameof(CharacterBody.isPlayerControlled)}"),
                x => x.MatchStloc(out _));
            c.Remove();
            c.EmitDelegate<Func<CharacterBody, bool>>(body => body.isPlayerControlled || body.GetComponent<FinalBossUpgradeStrategies.BodyBehavior>());
        }

        private static void GeodeShatter_SearchForPlayers(ILCursor c)
        {
            c.GotoNext(x => x.MatchCallvirt<SphereSearch>(nameof(SphereSearch.FilterCandidatesByHurtBoxTeam)));
            c.Remove();
            c.EmitDelegate<Func<SphereSearch, TeamMask, SphereSearch>>((sphereSearch, mask) => FilterCandidatesByHurtBoxTeamOrPredicate(sphereSearch, mask, hurtBox => hurtBox.healthComponent?.body?.GetComponent<FinalBossUpgradeStrategies.BodyBehavior>()));
        }

        private static SphereSearch FilterCandidatesByHurtBoxTeamOrPredicate(SphereSearch self, TeamMask mask, Func<HurtBox, bool> predicate)
        {
            self.searchData.FilterByHurtBoxes();
            for (int i = self.searchData.candidatesCount - 1; i >= 0; i--)
            {
                ref SphereSearch.Candidate candidate = ref self.searchData.GetCandidate(i);
                if (!mask.HasTeam(candidate.hurtBox.teamIndex) && !predicate(candidate.hurtBox))
                {
                    self.searchData.RemoveCandidate(i);
                }
            }
            return self;
        }

        private void GeodeInert_OnEnter(On.EntityStates.Geode.GeodeInert.orig_OnEnter orig, EntityStates.Geode.GeodeInert self)
        {
            orig(self);

            if (self.geodeController.ShouldRegenerate && MeridianEventTriggerInteraction.instance.TryGetComponent<FinalBossUpgradeStrategies.RepositionGeodesBehavior>(out var behavior))
            {
                behavior.GeodeBecameInert(self);
            }
        }

        private void Phase3_OnEnter(On.EntityStates.MeridianEvent.Phase3.orig_OnEnter orig, EntityStates.MeridianEvent.Phase3 self)
        {
            MeridianEventTriggerInteraction.instance.musicPhaseTwo.SetActive(false);
            MeridianEventTriggerInteraction.instance.musicPhaseThree.SetActive(true);
            orig(self);
        }

        private void BrokenCrystalDeathState_PlayDeathAnimation(On.EntityStates.FalseSonBoss.BrokenCrystalDeathState.orig_PlayDeathAnimation orig, BrokenCrystalDeathState self, float crossfadeDuration)
        {
            self.PlayAnimation("FullBody, Override", "Phase2Death", "StepBrothersPrep.playbackRate", BrokenCrystalDeathState.duration, 0f);
        }

        private void CrystalDeathState_PlayDeathAnimation(On.EntityStates.FalseSonBoss.CrystalDeathState.orig_PlayDeathAnimation orig, CrystalDeathState self, float crossfadeDuration)
        {
            self.PlayAnimation("FullBody, Override", "Phase1Death", "StepBrothersPrep.playbackRate", CrystalDeathState.duration, 0f);
        }

        private void FSBFPhaseBaseState_OnBossGroupDefeated(On.EntityStates.MeridianEvent.FSBFPhaseBaseState.orig_OnBossGroupDefeated orig, EntityStates.MeridianEvent.FSBFPhaseBaseState self, BossGroup bossGroup)
        {
            if (self is EntityStates.MeridianEvent.Phase2)
            {
                self.endStateDelay = 3;
            }

            orig(self, bossGroup);
            self.KillAllMonsters();

            switch (self)
            {
                case EntityStates.MeridianEvent.Phase1 _:
                    MeridianEventTriggerInteraction.instance.musicPhaseOne.SetActive(false);
                    MeridianEventTriggerInteraction.instance.musicPhaseTwo.SetActive(true);
                    break;

                case EntityStates.MeridianEvent.Phase2 _:
                    MeridianEventTriggerInteraction.instance.musicPhaseTwo.SetActive(false);
                    MeridianEventTriggerInteraction.instance.musicPhaseThree.SetActive(true);
                    break;
            }
        }

        private void Phase3_OnExit(On.EntityStates.MeridianEvent.Phase3.orig_OnExit orig, EntityStates.MeridianEvent.Phase3 self)
        {
            var repositionGeodesBehavior = MeridianEventTriggerInteraction.instance?.GetComponent<FinalBossUpgradeStrategies.RepositionGeodesBehavior>();

            if (repositionGeodesBehavior)
            {
                UnityEngine.Object.Destroy(repositionGeodesBehavior);
            }

            var colossusHead = MeridianEventTriggerInteraction.instance?.colossusHead;

            if (colossusHead && colossusHead!.TryGetComponent<FireLaserMore.FireLaserMoreBehavior>(out var behavior))
            {
                UnityEngine.Object.Destroy(behavior);
            }

            orig(self);
        }

        private bool BaseAI_HasLOS_Vector3(On.RoR2.CharacterAI.BaseAI.orig_HasLOS_Vector3 orig, RoR2.CharacterAI.BaseAI self, Vector3 end)
        {
            if (orig(self, end))
            {
                return true;
            }

            if (!IsFalseSonBoss(self.body))
            {
                return false;
            }

            var modelChildLocator = self.body.modelLocator.modelChildLocator;

            if (!modelChildLocator)
            {
                return false;
            }

            return self.HasLOS(modelChildLocator!.FindChild("Head").transform.position, end);
        }

        private void SpawnCard_onSpawnedServerGlobal(SpawnCard.SpawnResult obj)
        {
            if (Utils.TryGetCharacterBody(obj.spawnedInstance, out var body) && IsFalseSonBoss(body))
            {
                float currentTurnSpeed = body.characterDirection.turnSpeed;
                float desiredTurnSpeed = Mathf.Min(currentTurnSpeed, 360);
                this.Logger.LogDebug($"Changing False Son turn speed from {currentTurnSpeed} to {desiredTurnSpeed}");
                body.characterDirection.turnSpeed = desiredTurnSpeed;
                body.skillLocator.primary.onSkillChanged += this.Primary_onSkillChanged;

                foreach (var ai in body.master.AiComponents)
                {
                    // TODO: do I want to move any of these changes to the upgraded version? do I want to move the LoS check point to False Son's head rather than his chest?

                    // TODO: add an extra "Sprint After Target" skill driver which does not require line of sight and sprints to the last seen position then disables itself until line of sight is acquired (i.e. custom target)
                    foreach (var skillDriver in ai.GetSkillDrivers("FissureSlam"))
                    {
                        skillDriver.activationRequiresAimTargetLoS = false;
                        skillDriver.activationRequiresTargetLoS = false;
                        skillDriver.selectionRequiresTargetLoS = false;
                        skillDriver.driverUpdateTimerOverride = 2;
                        skillDriver.selectionRequiresOnGround = true;
                        skillDriver.maxDistance = 30;
                    }

                    foreach (var skillDriver in ai.GetSkillDrivers("SwatAwayPlayers"))
                    {
                        skillDriver.activationRequiresAimTargetLoS = false;
                        skillDriver.activationRequiresTargetLoS = false;
                        skillDriver.selectionRequiresTargetLoS = false;
                    }

                    foreach (var skillDriver in ai.GetSkillDrivers("Lunar Rain"))
                    {
                        skillDriver.activationRequiresAimTargetLoS = false;
                        skillDriver.activationRequiresTargetLoS = false;
                        skillDriver.selectionRequiresTargetLoS = false;
                    }

                    foreach (var skillDriver in ai.GetSkillDrivers("Sprint After Target"))
                    {
                        skillDriver.selectionRequiresTargetLoS = false;
                    }
                }
            }
        }

        private void Phase2_OnExit(On.EntityStates.MeridianEvent.Phase2.orig_OnExit orig, EntityStates.MeridianEvent.Phase2 self)
        {
            if (self.meridianEventTriggerInteraction?.phase2CombatDirector && self.meridianEventTriggerInteraction.phase2CombatDirector.TryGetComponent<FinalBossUpgradeStrategies.MiniBossSpawner>(out var behavior))
            {
                UnityEngine.Object.Destroy(behavior);
            }

            orig(self);
        }
    }
}
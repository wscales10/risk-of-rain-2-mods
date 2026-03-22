using EntityStates;
using EntityStates.FalseSonBoss;
using HG;
using MonoMod.Cil;
using PactOfPunishment.Waves.Common;
using RoR2;
using RoR2.CharacterAI;
using System;
using System.Linq;
using UnityEngine;

namespace PactOfPunishment.Waves.FinalStage
{
    public class UpgradedLeap : Module
    {
        public interface IUpgradedLeapController
        {
            void TryUpgradedLeap(UpgradedLeapBodyBehavior sender);
        }

        public override void Init()
        {
            On.EntityStates.FalseSonBoss.LunarGazeLeap.OnEnter += this.LunarGazeLeap_OnEnter;
            // IL.EntityStates.GenericCharacterDeath.OnEnter += Utils.HookIL(GenericCharacterDeath_OnEnter);
            On.EntityStates.FalseSonBoss.CorruptedPathsDash.OnExit += this.CorruptedPathsDash_OnExit;
            On.EntityStates.FalseSonBoss.HeartSpawnState.OnEnter += this.HeartSpawnState_OnEnter;
        }

        private static void GenericCharacterDeath_OnEnter(ILCursor c)
        {
            c.GotoNext(x => x.MatchCallvirt<CharacterBody>($"get_{nameof(CharacterBody.isGlass)}"));
            c.Remove();
            c.EmitDelegate<Func<CharacterBody, bool>>(body => body.isGlass || body.GetComponent<UpgradedLeapGhostBehavior>());
        }

        private void HeartSpawnState_OnEnter(On.EntityStates.FalseSonBoss.HeartSpawnState.orig_OnEnter orig, HeartSpawnState self)
        {
            if (self.GetComponent<UpgradedLeapGhostBehavior>())
            {
                self.baseAnimDuration = 1.5f;
                self.baseSpawnDuration = 1.5f;
            }

            orig(self);
        }

        private void CorruptedPathsDash_OnExit(On.EntityStates.FalseSonBoss.CorruptedPathsDash.orig_OnExit orig, CorruptedPathsDash self)
        {
            orig(self);
            if (self.TryGetComponent<UpgradedLeapGhostBehavior>(out var behavior))
            {
                behavior.OnDashEnd();
            }
        }

        private void LunarGazeLeap_OnEnter(On.EntityStates.FalseSonBoss.LunarGazeLeap.orig_OnEnter orig, EntityStates.FalseSonBoss.LunarGazeLeap self)
        {
            orig(self);

            if (self.characterBody.TryGetComponent<UpgradedLeapBodyBehavior>(out var behavior))
            {
                behavior.OnLeapStart();
            }
        }

        public class UpgradedLeapBodyBehavior : BossBodyBehavior
        {
            public IUpgradedLeapController? Controller;

            internal void OnLeapStart()
            {
                if (!(this.Body && this.Body.enabled && this.Body.healthComponent))
                {
                    return;
                }

                if (this.Body.healthComponent.health + this.Body.healthComponent.shield < 0.7f * this.Body.healthComponent.fullCombinedHealth)
                {
                    this.Controller?.TryUpgradedLeap(this);
                }
            }
        }

        public class UpgradedLeapGhostBehavior : BossBodyBehavior
        {
            private enum State
            {
                WaitingToSlam,
                Slamming,
                PostSlam,
                Death
            }

            private State currentState;

            public CharacterBody? TargetBody;

            private EntityStateMachine bodyStateMachine;

            public Vector3? StandPosition { get; private set; }

            private FinalBoss.PredictiveDashController predictiveDashController;

            private float deathTimer;

            internal void OnDashEnd()
            {
                var horizontalFacingDirection = (this.TargetBody!.corePosition - this.Body.corePosition).normalized;
                this.Body.characterDirection.forward = horizontalFacingDirection;
                this.Body.inputBank.aimDirection = horizontalFacingDirection;
            }

            internal void OnDashEnter()
            {
                var targetBodyObject = this.TargetBody!.gameObject;
                this.Body.master.GetComponent<BaseAI>().currentEnemy.gameObject = targetBodyObject;
            }

            protected override void Awake()
            {
                base.Awake();
                this.predictiveDashController = this.EnsureComponent<FinalBoss.PredictiveDashController>();
                this.predictiveDashController.GetLookAheadDuration = () => 0.9f + FinalBoss.GetPreFissureSlamDuration(this.Body);
                this.predictiveDashController.GetDashTargetPositionFromPredictedPosition = this.GetDashTargetPositionFromPredictedPosition;

                this.bodyStateMachine = EntityStateMachine.FindByCustomName(this.gameObject, "Body");
                (this.Body.inventory ??= this.Body.master.inventory).GiveItemPermanent(RoR2Content.Items.Ghost);

                var ai = this.Body.master.GetComponent<BaseAI>();
                ai.ReplaceSkillDrivers(Array.Empty<AISkillDriver>());

                // Remove death animation
                // this.Body.GetComponent<CharacterDeathBehavior>().deathState = new SerializableEntityStateType(typeof(GenericCharacterDeath));
                // this.Body.healthComponent.NetworkdestroyModelOnDeath = true;
            }

            private Vector3 GetDashTargetPositionFromPredictedPosition(Vector3 predictedPosition)
            {
                if (Physics.Raycast(new Vector3(predictedPosition.x, this.Body.aimOrigin.y, predictedPosition.z), Vector3.down, out var hitInfo, 1000, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                {
                    predictedPosition = hitInfo.point;
                }

                var nodesWithinRange = SceneInfo.instance.groundNodes.FindNodesInRange(predictedPosition, 0, 15, HullMask.Golem);

                Vector3? furthestNodePosition = null;
                float furthestNodeDistance = 0;

                foreach (var nodeIndex in nodesWithinRange)
                {
                    SceneInfo.instance.groundNodes.GetNodePosition(nodeIndex, out var nodePosition);
                    float nodeDistance = Vector3.Distance(nodePosition, this.TargetBody!.footPosition);

                    if (nodeDistance > furthestNodeDistance)
                    {
                        furthestNodePosition = nodePosition;
                        furthestNodeDistance = nodeDistance;
                    }
                }

                Vector3 standPosition;

                if (furthestNodePosition.HasValue)
                {
                    standPosition = furthestNodePosition.Value;
                }
                else
                {
                    Debug.LogWarning("Failed to find a good node at which to place ghost");
                    SceneInfo.instance.groundNodes.GetNodePosition(SceneInfo.instance.groundNodes.FindClosestNode(predictedPosition, HullClassification.Golem), out var closestNodePosition);
                    standPosition = closestNodePosition;
                }

                Vector3 horizontalFacingDirection = this.TargetBody!.footPosition - standPosition;
                horizontalFacingDirection.y = 0;

                this.StandPosition = standPosition;
                return standPosition;
            }

            protected override void ManagedFixedUpdate(float deltaTime)
            {
                base.ManagedFixedUpdate(deltaTime);

                if (this.Body?.characterDirection)
                {
                    this.Body!.characterDirection.turnSpeed = 2000;
                }

                if (this.StandPosition is null && this.bodyStateMachine.IsInMainState())
                {
                    this.SetupDash();
                }

                switch (this.currentState)
                {
                    case State.WaitingToSlam:
                        if (this.bodyStateMachine.state is FissureSlam || this.bodyStateMachine.state is FissureSlamWindup)
                        {
                            this.currentState = State.Slamming;
                        }
                        break;
                    case State.Slamming:
                        if (!(this.bodyStateMachine.state is FissureSlam || this.bodyStateMachine.state is FissureSlamWindup))
                        {
                            this.currentState = State.PostSlam;
                        }
                        break;
                    case State.PostSlam:
                        this.deathTimer += deltaTime;

                        if (this.deathTimer > 0)
                        {
                            this.currentState = State.Death;
                        }
                        break;
                    case State.Death:
                        this.Body.master.TrueKill();
                        this.enabled = false;
                        break;
                }
            }

            private void SetupDash()
            {
                if (this.TargetBody)
                {
                    this.predictiveDashController.TargetBodyOverride = this.TargetBody;
                    this.bodyStateMachine.SetNextState(Utils.InstantiateState<CorruptedPathsDash>());
                }
            }
        }
    }
}
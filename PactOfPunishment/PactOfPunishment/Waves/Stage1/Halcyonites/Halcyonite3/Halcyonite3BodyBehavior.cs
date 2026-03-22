using EntityStates;
using EntityStates.Halcyonite;
using HG;
using PactOfPunishment.Waves.Halcyonites;
using R2API;
using RoR2;
using UnityEngine;

namespace PactOfPunishment.Waves.Stage1.Halcyonites.Halcyonite3
{
    public class Halcyonite3BodyBehavior : HalcyoniteBodyBehavior, IModifyOverlapAttack
    {
        public GameObject? DustCenterPrefab;

        public Xoroshiro128Plus? rng;

        public bool isBurstLaserEnabled;

        public bool isThreeWayFistsEnabled;

        private static float meleeAttackPushAwayForceMultiplier = 0.5f;

        private LaserMode currentLaserMode;

        private TriLaserModule.TriLaserStats laserModifier;

        public enum LaserMode
        {
            Disrupt,

            Burst
        }

        public enum ThrustContext
        {
            PostLaser,

            FirstOfTwo,

            SecondOfTwo
        }

        public enum PostThrustState
        {
            Main,

            Thrust,

            Slash
        }

        public LaserMode CurrentLaserMode => this.currentLaserMode;

        public ThrustContext CurrentThrustContext { get; private set; } // TODO: do I need this?

        public PostThrustState CurrentPostThrustState { get; private set; }

        public static void OnRecalculateStats(RecalculateStatsAPI.StatHookEventArgs args)
        {
            args.damageTotalMult *= 0.8f;
            args.secondarySkill.bonusStockAdd++;
        }

        public LaserMode ChooseNewLaserMode()
        {
            if (!this.isBurstLaserEnabled)
            {
                return this.currentLaserMode = LaserMode.Disrupt;
            }

            return this.currentLaserMode = this.rng?.NextEnum<LaserMode>() ?? EnumUtils.Random<LaserMode>();
        }

        public void Thrusted()
        {
            switch (this.CurrentPostThrustState)
            {
                case PostThrustState.Main:
                    break;

                case PostThrustState.Thrust:
                    this.CurrentPostThrustState = PostThrustState.Slash;
                    this.CurrentThrustContext = ThrustContext.SecondOfTwo;
                    break;

                default:
                    this.CurrentPostThrustState = PostThrustState.Main;
                    break;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            this.Body.skillLocator.primary.cooldownOverride = 12;
            RecalculateStats.SetMinimumInterruptPriorityOverride(this.WeaponStateMachine, typeof(GoldenSwipe), _ => InterruptPriority.PrioritySkill);
            RecalculateStats.SetMinimumInterruptPriorityOverride(this.WeaponStateMachine, typeof(GoldenSlash), _ => InterruptPriority.PrioritySkill);
            RecalculateStats.SetMinimumInterruptPriorityOverride(this.WeaponStateMachine, typeof(EntityStates.ImpBossMonster.BlinkState), _ => InterruptPriority.Death);
            this.EnsureComponent<HalcyoniteThrustBehavior>();
            this.laserModifier = this.Body.EnsureComponent<TriLaserModule.StateModifier>().Stats;
            this.laserModifier.BaseTotalTimesToFire = 1;

            var stateMachine = this.gameObject.AddComponent<EntityStateMachine>();
            stateMachine.customName = "BossBody";
            this.Body?.healthComponent.ForwardBossDamageTo(stateMachine);
            stateMachine.SetState(new Halcyonite3States.Phase1());
        }

        public void OnEnable()
        {
            this.Body.onSkillActivatedAuthority += this.Body_onSkillActivatedAuthority;
            RecalculateStats.Add(this.Body, OnRecalculateStats);
        }

        public void OnDisable()
        {
            this.Body.onSkillActivatedAuthority -= this.Body_onSkillActivatedAuthority;
            RecalculateStats.Remove(this.Body, OnRecalculateStats);
        }

        public void ModifyOverlapAttack(BasicMeleeAttack state)
        {
            state.forceVector = Vector3.up * state.forceVector.y;
            state.pushAwayForce *= meleeAttackPushAwayForceMultiplier;

            if (state.overlapAttack != null)
            {
                state.overlapAttack.forceVector = Vector3.up * state.overlapAttack.forceVector.y;
                state.overlapAttack.pushAwayForce *= meleeAttackPushAwayForceMultiplier;
            }
        }

        // Executes before the state changes
        private void Body_onSkillActivatedAuthority(GenericSkill obj)
        {
            switch (this.Body.skillLocator.FindSkillSlot(obj))
            {
                // Thrust
                case SkillSlot.Primary:
                    this.CurrentPostThrustState = PostThrustState.Thrust;
                    this.CurrentThrustContext = ThrustContext.FirstOfTwo;
                    break;

                // Laser
                case SkillSlot.Secondary:
                    this.CurrentPostThrustState = PostThrustState.Main;
                    this.CurrentThrustContext = ThrustContext.PostLaser;
                    switch (this.ChooseNewLaserMode())
                    {
                        case LaserMode.Disrupt:
                            this.laserModifier.ChargeTimeMultiplier = 0.4f;
                            this.laserModifier.BaseTotalTimesToFire = 1;
                            this.laserModifier.DamageMultiplier = 0.3f;
                            break;

                        default:
                            this.laserModifier.ChargeTimeMultiplier = 1;
                            this.laserModifier.BaseTotalTimesToFire = 3;
                            this.laserModifier.DamageMultiplier = 1;
                            break;
                    }
                    break;

                default:
                    this.CurrentPostThrustState = PostThrustState.Main;
                    break;
            }
        }
    }
}
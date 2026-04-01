using EntityStates;
using HG;
using PactOfPunishment.Waves.Halcyonites;
using R2API;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Skills;
using UnityEngine;

namespace PactOfPunishment.Waves.Stage3
{
    public class FinalHalcyoniteBodyBehavior : HalcyoniteBodyBehavior, ILifeBehavior
    {
        private static AssetPromise<SkillDef> upgradedLaserSkillDef;

        private TriLaserModule.TriLaserStats triLaserModifier;

        private EntityStateMachine stateMachine;

        public enum State
        {
            None,

            Collective,

            CollectivePlus,

            Gilded
        }

        public State CurrentState { get; private set; }

        public State DesiredState { get; set; }

        public InterruptPriority GetSkillInterruptPriority(SkillDef self)
        {
            if (self == HalcyoniteModule.WhirlwindSkillDef && this.CurrentState == State.Gilded)
            {
                return InterruptPriority.PrioritySkill;
            }

            return self.interruptPriority;
        }

        public void Update()
        {
            if (this.CurrentState != this.DesiredState)
            {
                this.SetState(this.DesiredState);
            }
        }

        public void OnEnable()
        {
            RecalculateStats.Add(this.Body, this.OnRecalculateStats);
        }

        public void OnDisable()
        {
            RecalculateStats.Remove(this.Body, this.OnRecalculateStats);
        }

        public void OnDeathStart()
        {
            Debug.Log($"Final Halcyonite body dying in state '{this.stateMachine?.state?.GetType().Name}'");
        }

        internal static void SetupSkillDef()
        {
            upgradedLaserSkillDef = Utils.BeginLoadAndTransform<SkillDef, SkillDef>("RoR2/DLC2/Halcyonite/HalcyoniteMonsterTriLaser.asset", x =>
            {
                var copy = Instantiate(x);
                copy.cancelSprintingOnActivation = false;
                copy.baseRechargeInterval = 0;
                copy.forceSprintDuringState = true;
                return copy;
            });
        }

        protected override void Awake()
        {
            base.Awake();
            this.Body.ScaleDifficultyAsBoss(2, 15, true, false);
            this.Body.inventory.GiveItemPermanent(RoR2Content.Items.AdaptiveArmor);
            this.Body.DisableStunsEtc();
            this.triLaserModifier = this.EnsureComponent<TriLaserModule.StateModifier>().Stats;
        }

        private void OnRecalculateStats(RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (this.CurrentState == State.Gilded)
            {
                args.sprintSpeedAdd += .3f;
            }
        }

        private void SetState(State state)
        {
            this.CurrentState = state;

            switch (state)
            {
                case State.None:
                    Debug.LogError("Setting state to None not supported.");
                    break;

                case State.Collective:
                    CommonCollectiveSetup();
                    break;

                case State.CollectivePlus:
                    CommonCollectiveSetup();
                    FistsController.Instance.AddHalcyonite(this.Body.master); // TODO: remove if state is not this
                    break;

                case State.Gilded:
                    Utils.MakeUnscaledEliteUsingEquipment(this.Body, DLC2Content.Elites.Aurelionite);
                    this.triLaserModifier.BaseTotalTimesToFire = 1;
                    this.triLaserModifier.KeepFiringWhileKeyDown = true;
                    this.triLaserModifier.FireCooldownOverride = 0.1f;
                    this.triLaserModifier.ChargeTimeMultiplier = 1 / 3f;
                    this.triLaserModifier.DamageMultiplier = 0.25f;
                    this.Body.skillLocator.secondary.SetSkillOverride(this, upgradedLaserSkillDef.Value, GenericSkill.SkillOverridePriority.Upgrade);

                    foreach (var ai in this.Body.master.AiComponents)
                    {
                        ai.aimVectorMaxSpeed = 72;
                        ai.aimVectorDampTime = 0.2f;

                        foreach (var skillDriver in ai.GetSkillDrivers("TriLaser"))
                        {
                            skillDriver.minDistance = 0;
                            skillDriver.noRepeat = false;
                            skillDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
                            skillDriver.shouldSprint = true;
                        }
                    }

                    Utils.MakeUnscaledEliteUsingEquipment(this.Body, DLC2Content.Elites.Aurelionite);
                    this.DisableSkill(this.Body, SkillSlot.Primary);
                    this.DisableSkill(this.Body, SkillSlot.Special);
                    break;
            }

            void CommonCollectiveSetup()
            {
                Utils.MakeUnscaledEliteUsingEquipment(this.Body, DLC3Content.Elites.Collective);
                this.triLaserModifier.BaseTotalTimesToFire = 2;
                this.triLaserModifier.KeepFiringWhileKeyDown = false;
                this.triLaserModifier.FireCooldownOverride = null;
                this.triLaserModifier.ChargeTimeMultiplier = 1;
                this.triLaserModifier.DamageMultiplier = 1;
                this.Body.skillLocator.secondary.UnsetSkillOverride(this, upgradedLaserSkillDef.Value, GenericSkill.SkillOverridePriority.Upgrade);

                foreach (var ai in this.Body.master.AiComponents)
                {
                    ai.aimVectorMaxSpeed = 360;
                    ai.aimVectorDampTime = 0.1f;

                    foreach (var skillDriver in ai.GetSkillDrivers("TriLaser"))
                    {
                        skillDriver.minDistance = 30;
                        skillDriver.noRepeat = true;
                        skillDriver.movementType = AISkillDriver.MovementType.Stop;
                        skillDriver.shouldSprint = false;
                    }
                }

                this.Body.MakeUnscaledEliteUsingEquipment(DLC3Content.Elites.Collective);
                this.EnableSkill(this.Body, SkillSlot.Primary);
                this.EnableSkill(this.Body, SkillSlot.Special);
            }
        }
    }
}
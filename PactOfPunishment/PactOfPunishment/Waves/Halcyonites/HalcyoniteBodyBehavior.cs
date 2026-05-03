using EntityStates.Halcyonite;
using HG;
using PactOfPunishment.AiSkillDrivers;
using PactOfPunishment.ProtectMonstersFromHazards;
using PactOfPunishment.Waves.Common;
using RoR2;
using RoR2.CharacterAI;
using System;
using System.Linq;

namespace PactOfPunishment.Waves.Halcyonites
{
    public class HalcyoniteBodyBehavior : BossBodyBehavior
    {
        private FallRiskMitigator fallRiskMitigator;

        public EntityStateMachine? WeaponStateMachine { get; private set; }

        public EntityStateMachine BossStateMachine { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            this.fallRiskMitigator = this.EnsureComponent<FallRiskMitigator>();
            this.fallRiskMitigator.CurrentMode = FallRiskMitigator.Mode.Halcyonite;
            this.WeaponStateMachine = EntityStateMachine.FindByCustomName(this.gameObject, "Weapon");

            this.EnsureComponent<WhirlWindNavigationController>();
            this.EnsureComponent<WhirlWindModule.OverrideGetTarget>();
            this.SetupBossAi();
            this.BossStateMachine = this.gameObject.AddComponent<EntityStateMachine>();
            this.BossStateMachine.customName = "BossBody";
            this.Body.healthComponent.ForwardBossDamageTo(this.BossStateMachine);

            // TODO: DRY, also add this stuff to other bosses
            foreach (var skillDriver in this.Body.GetSkillDrivers("SeekSafeWard"))
            {
                skillDriver.ignoreNodeGraph = true;
            }

            this.EnsureComponent<ObstacleNavigator>();
        }

        protected override void ManagedFixedUpdate(float deltaTime)
        {
            base.ManagedFixedUpdate(deltaTime);

            if (!this.Body)
            {
                this.fallRiskMitigator.DoUpdate(null);
                return;
            }

            this.fallRiskMitigator.DoUpdate(this.Body!.transform);

            var utilitySkill = this.Body!.skillLocator.utility;
            if (this.fallRiskMitigator.IsAboveGround == false)
            {
                switch (this.WeaponStateMachine?.state)
                {
                    case WhirlWindPersuitCycle _:
                    case WhirlwindWarmUp _:
                        break;

                    default:
                        if (this.WeaponStateMachine && this.WeaponStateMachine!.SetInterruptState(Utils.InstantiateState<WhirlwindWarmUp>(), EntityStates.InterruptPriority.PrioritySkill) && utilitySkill?.skillDef == HalcyoniteModule.WhirlwindSkillDef)
                        {
                            utilitySkill?.DeductStock(utilitySkill.skillDef.stockToConsume);
                        }

                        break;
                }
            }
        }

        protected virtual void SetupThrustSkillDriver(AISkillDriver skillDriver)
        {
        }

        protected virtual void SetupLaserSkillDriver(AISkillDriver skillDriver)
        {
        }

        protected virtual void SetupWhirlWindSkillDriver(AISkillDriver whirlwindSkillDriver)
        {
            // Increase max activation distance of whirlwind, so the Halcyonite doesn't get stuck if
            // far from the NodeGraph.
            whirlwindSkillDriver.maxDistance = float.MaxValue;
            whirlwindSkillDriver.activationRequiresAimConfirmation = false;
            whirlwindSkillDriver.selectionRequiresTargetLoS = false;
            whirlwindSkillDriver.activationRequiresTargetLoS = false;

            // Disable this behavior if new skill is active
            whirlwindSkillDriver.requiredSkill = HalcyoniteModule.WhirlwindSkillDef; // TODO: check this, maybe loading asset is not the correct way.
        }

        protected virtual void SetupBossAi(BaseAI ai)
        {
            ai.prioritizePlayers = true;
            ai.fullVision = true;
            ai.xrayVision = true;

            foreach (var skillDriver in ai.GetSkillDrivers("Golden Swipe"))
            {
                this.SetupThrustSkillDriver(skillDriver);
            }

            foreach (var skillDriver in ai.GetSkillDrivers("TriLaser"))
            {
                this.SetupLaserSkillDriver(skillDriver);
            }

            int index = Array.FindIndex(ai.skillDrivers, x => x.customName == "WhirlwindRush");

            if (index != -1)
            {
                TryToEscapeFog.Instance.InsertSkillDriver(ai, newSkillDriver =>
                {
                    newSkillDriver.customName = "WhirlWindToSafeWard";
                    newSkillDriver.skillSlot = SkillSlot.Utility;
                    newSkillDriver.requiredSkill = HalcyoniteModule.WhirlwindSkillDef;
                    newSkillDriver.requireSkillReady = true;
                    newSkillDriver.ignoreNodeGraph = true;
                    newSkillDriver.driverUpdateTimerOverride = 4;
                }, index);
                this.SetupWhirlWindSkillDriver(ai.skillDrivers[index]);
            }
        }

        protected void SetupBossAi()
        {
            foreach (var ai in this.Body.master.AiComponents.Where(x => x))
            {
                this.SetupBossAi(ai);
            }
        }
    }
}
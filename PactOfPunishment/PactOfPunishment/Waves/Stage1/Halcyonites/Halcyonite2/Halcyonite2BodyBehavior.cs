using EntityStates.Halcyonite;
using HG;
using PactOfPunishment.AiSkillDrivers;
using PactOfPunishment.Waves.Halcyonites;
using R2API;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Projectile;
using System;
using UnityEngine;

namespace PactOfPunishment.Waves.Stage1.Halcyonites.Halcyonite2
{
    public class PowerMeter
    {
        private static float chargeTime = 10;

        private static float dischargeTime = 13;

        private static float quickChargeTime = 0.2f;

        public event Action? IsPoweredChanged;

        public bool IsPoweredUp { get; set; }

        public float Power { get; private set; }

        public bool Persistent { get; set; }

        public void FixedUpdate(float deltaTime, bool isPoweringUp)
        {
            if (this.IsPoweredUp)
            {
                if (!this.Persistent)
                {
                    this.Power -= deltaTime / dischargeTime;
                }

                if (this.Power <= 0)
                {
                    this.Power = 0;
                    this.IsPoweredUp = false;
                    this.IsPoweredChanged?.Invoke();
                }
            }
            else
            {
                if (this.Persistent)
                {
                    this.Power += deltaTime / quickChargeTime;
                }
                else if (isPoweringUp)
                {
                    this.Power += deltaTime / chargeTime;
                }

                if (this.Power >= 1)
                {
                    this.Power = 1;
                    this.IsPoweredUp = true;
                    this.IsPoweredChanged?.Invoke();
                }
            }
        }
    }

    public class Halcyonite2BodyBehavior : Stage1HalcyoniteBodyBehavior
    {
        public static GameObject PillarPrefab;

        public readonly PowerMeter powerMeter = new PowerMeter();

        public CombatDirector? CombatDirector;

        public bool SpawnPillars;

        private GenericSkill newSkill;

        private bool canUseNewSkill;

        private bool haveUsedWhirlWindSinceLastUsingNewSkill;

        public bool CanUseNewSkill
        {
            get => this.canUseNewSkill;

            set
            {
                if (this.canUseNewSkill != value)
                {
                    this.canUseNewSkill = value;
                    this.SelectUtilitySkill();
                }
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

        internal void SetupSkill(GenericSkill skill)
        {
            skill._skillFamily = CustomWeaponStates.RepeatingFistSkillState.skillFamily;
        }

        protected override void Awake()
        {
            base.Awake();
            this.Body.ScaleDifficultyAsBoss(new BossScalingArgs1(0.62f, 65f, false, 10), false);
            Utils.ScaleDeathRewards(this.Body, Utils.CreditsForBossWave(10) / 200);
            this.Body.DisableStunsEtc();
            this.powerMeter.IsPoweredChanged += this.PowerMeter_IsPoweredChanged;
            this.Body.skillLocator.primary.cooldownOverride = 6;
            this.Body.skillLocator.secondary.cooldownOverride = 12;
            this.Body.onSkillActivatedServer += this.Body_onSkillActivatedServer;
            var thrustBehavior = this.EnsureComponent<HalcyoniteThrustBehavior>();
            thrustBehavior.getDesiredDistance = () => 16;
            thrustBehavior.OnThrust += this.ThrustBehavior_OnThrust;

            var laserModifier = this.Body.EnsureComponent<TriLaserModule.StateModifier>().Stats;
            laserModifier.BaseTotalTimesToFire = 4;
            laserModifier.FireCooldownOverride = 0.75f;
            laserModifier.EndLagOverride = 2;
            laserModifier.ChargeTimeMultiplier = 1 / 3f;
            laserModifier.KeepFiringWhileKeyDown = true;

            this.newSkill = this.gameObject.AddComponent<GenericSkill>();
        }

        protected override void ManagedFixedUpdate(float deltaTime)
        {
            base.ManagedFixedUpdate(deltaTime);
            var state = this.WeaponStateMachine?.state;
            this.powerMeter.FixedUpdate(deltaTime, state is TriLaser);
        }

        protected override void SetupBossAi(BaseAI ai)
        {
            base.SetupBossAi(ai);

            CustomWeaponStates.RepeatingFistSkillState.customSkill.InsertSkillDriver(ai, Array.FindLastIndex(ai.skillDrivers, sd => sd.skillSlot == SkillSlot.Utility) + 1);

            foreach (var skillDriver in ai.GetSkillDrivers("Follow Target"))
            {
                skillDriver.shouldSprint = true;
            }
        }

        protected override void SetupLaserSkillDriver(AISkillDriver skillDriver)
        {
            base.SetupLaserSkillDriver(skillDriver);
            skillDriver.movementType = AISkillDriver.MovementType.Stop;
            skillDriver.moveInputScale = 0;
            skillDriver.requiredSkill = HalcyoniteModule.LaserSkillDef.Value;
            skillDriver.noRepeat = false;
        }

        private void ThrustBehavior_OnThrust()
        {
            if (this.SpawnPillars)
            {
                ProjectileManager.instance.FireProjectileWithoutDamageType(
                    PillarPrefab,
                    this.Body.footPosition,
                    Quaternion.identity,
                    this.gameObject,
                    this.Body.damage * 3,
                    0f,
                    Util.CheckRoll(this.Body.crit, this.Body.master));
            }
        }

        private void Body_onSkillActivatedServer(GenericSkill skill)
        {
            if (skill == this.DefaultUtilitySkill)
            {
                this.haveUsedWhirlWindSinceLastUsingNewSkill = true;
                this.SelectUtilitySkill();
            }
            else if (skill == this.newSkill)
            {
                this.haveUsedWhirlWindSinceLastUsingNewSkill = false;
                this.SelectUtilitySkill();
            }
        }

        private void SelectUtilitySkill()
        {
            // Always enable the new skill while powered up - the AISkillDriver will not use it
            // above 75%
            this.SetNewSkillActive(this.powerMeter.IsPoweredUp || (this.haveUsedWhirlWindSinceLastUsingNewSkill && this.CanUseNewSkill));
        }

        private void SetNewSkillActive(bool active)
        {
            if (active)
            {
                this.Body.skillLocator.utility = this.newSkill;
            }
            else
            {
                this.Body.skillLocator.utility = this.DefaultUtilitySkill;
            }
        }

        private void PowerMeter_IsPoweredChanged()
        {
            if (this.powerMeter.IsPoweredUp)
            {
                // Disable laser while powered up
                this.DisableSkill(this.Body, SkillSlot.Secondary);

                this.Body.MakeUnscaledEliteUsingEquipment(RoR2Content.Elites.Fire);

                var weaponState = this.WeaponStateMachine?.state;

                if (weaponState is TriLaser || weaponState is ChargeTriLaser)
                {
                    this.WeaponStateMachine!.SetNextStateToMain();
                }
            }
            else
            {
                this.Body.inventory.SetEquipmentIndex(EquipmentIndex.None, true);
                this.EnableSkill(this.Body, SkillSlot.Secondary);
            }

            this.SelectUtilitySkill();
        }

        private void OnRecalculateStats(RecalculateStatsAPI.StatHookEventArgs args)
        {
            args.primarySkill.bonusStockAdd++;

            if (this.powerMeter.IsPoweredUp)
            {
                args.attackSpeedTotalMult /= 0.7f;
                args.moveSpeedTotalMult /= 0.7f;
                args.allSkills.cooldownMultiplier *= 0.7f;
            }

            var state = this.WeaponStateMachine?.state;
            if (state is TriLaser || state is ChargeTriLaser)
            {
                args.moveSpeedTotalMult = 0;
            }
        }
    }
}
using EntityStates.Halcyonite;
using HG;
using PactOfPunishment.Waves.Common;
using RoR2;
using RoR2.CharacterAI;
using System.Linq;
using UnityEngine;

namespace PactOfPunishment.Waves.Halcyonites
{
    public class HalcyoniteBodyBehavior : BossBodyBehavior
    {
        private FallRiskMitigator fallRiskMitigator;

        public EntityStateMachine? WeaponStateMachine { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            this.fallRiskMitigator = this.EnsureComponent<FallRiskMitigator>();
            this.fallRiskMitigator.CurrentMode = FallRiskMitigator.Mode.Halcyonite;
            this.WeaponStateMachine = EntityStateMachine.FindByCustomName(this.gameObject, "Weapon");
            this.EnsureComponent<WhirlWindModule.UseAirNodes>();
            this.EnsureComponent<WhirlWindModule.OverrideGetTarget>();
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
            if (this.fallRiskMitigator.IsAboveGround == false && utilitySkill.skillDef == HalcyoniteModule.WhirlwindSkillDef)
            {
                switch (this.WeaponStateMachine?.state)
                {
                    case WhirlWindPersuitCycle _:
                    case WhirlwindWarmUp _:
                        break;

                    default:
                        utilitySkill.stock = utilitySkill.skillDef.requiredStock;

                        foreach (var ai in this.Body?.master?.AiComponents ?? Enumerable.Empty<BaseAI>())
                        {
                            ai.skillDriverUpdateTimer = Mathf.Min(ai.skillDriverUpdateTimer, 0.15f);
                        }

                        break;
                }
            }
        }
    }
}
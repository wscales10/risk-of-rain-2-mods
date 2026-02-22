using EntityStates.Halcyonite;
using PactOfPunishment.Waves.Stage1.Halcyonites;
using RoR2;
using RoR2.CharacterAI;
using System.Linq;
using UnityEngine;

namespace PactOfPunishment.Waves.Common
{
    public class HalcyoniteBodyBehavior : MonoBehaviour
    {
        private EntityStateMachine? weaponStateMachine;

        public bool? IsAboveGround { get; private set; }

        protected CharacterBody? Body { get; private set; }

        public virtual void Awake()
        {
            this.Body = this.GetComponent<CharacterBody>();
            this.weaponStateMachine = EntityStateMachine.FindByCustomName(this.gameObject, "Weapon");
        }

        public void FixedUpdate()
        {
            this.FixedUpdate(Time.fixedDeltaTime);
        }

        protected virtual void FixedUpdate(float deltaTime)
        {
            if (!this.Body)
            {
                this.IsAboveGround = null;
                return;
            }

            this.IsAboveGround = HalcyoniteModule.IsAboveGroundInternal(this.Body!.transform);

            var utilitySkill = this.Body!.skillLocator.utility;
            if (this.IsAboveGround == false && utilitySkill.skillDef == HalcyoniteModule.WhirlwindSkillDef)
            {
                switch (this.weaponStateMachine?.state)
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
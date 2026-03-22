using PactOfPunishment.Waves.Common;
using RoR2;
using RoR2.CharacterAI;
using System;
using System.Linq;

namespace PactOfPunishment.Waves.Halcyonites
{
    public abstract class HalcyoniteBossFightBehavior : BossFightBehavior
    {
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
                this.SetupWhirlWindSkillDriver(ai.skillDrivers[index]);
            }
        }

        protected void SetupBossAi(CharacterBody body)
        {
            foreach (var ai in body.master.AiComponents.Where(x => x))
            {
                this.SetupBossAi(ai);
            }
        }
    }
}
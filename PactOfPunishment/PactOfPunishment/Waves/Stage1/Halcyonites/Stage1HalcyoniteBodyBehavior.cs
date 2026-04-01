using HG;
using PactOfPunishment.Waves.Halcyonites;
using RoR2.CharacterAI;
using System;

namespace PactOfPunishment.Waves.Stage1.Halcyonites
{
    public class Stage1HalcyoniteBodyBehavior : HalcyoniteBodyBehavior
    {
        protected override void SetupThrustSkillDriver(AISkillDriver skillDriver)
        {
            base.SetupThrustSkillDriver(skillDriver);

            // Increase max activation distance of thrust, as it will move the Halcyonite forward
            skillDriver.maxDistance += 16;
        }

        protected override void SetupLaserSkillDriver(AISkillDriver skillDriver)
        {
            base.SetupLaserSkillDriver(skillDriver);
            skillDriver.selectionRequiresOnGround = true;
            skillDriver.moveInputScale = 0;
            skillDriver.minDistance = 0;
        }

        protected override void SetupBossAi(BaseAI ai)
        {
            base.SetupBossAi(ai);

            int index = Array.FindIndex(ai.skillDrivers, x => x.customName == "WhirlwindRush");

            if (index != -1)
            {
                int laserIndex = Array.FindIndex(ai.skillDrivers, x => x.customName == "TriLaser");

                if (laserIndex != -1)
                {
                    ArrayUtils.Swap(ai.skillDrivers, index, laserIndex);
                }
            }
        }

        protected override void SetupWhirlWindSkillDriver(AISkillDriver whirlwindSkillDriver)
        {
            base.SetupWhirlWindSkillDriver(whirlwindSkillDriver);

            // Increase min activation distance of whirlwind, so the Halcyonite uses thrust instead
            // more often
            whirlwindSkillDriver.minDistance += 10;
        }
    }
}
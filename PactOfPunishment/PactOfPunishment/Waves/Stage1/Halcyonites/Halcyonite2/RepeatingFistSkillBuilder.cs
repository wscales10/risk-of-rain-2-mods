using BepInEx.Logging;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Skills;
using static PactOfPunishment.Waves.Stage1.Halcyonites.CustomWeaponStates;

namespace PactOfPunishment.Waves.Stage1.Halcyonites.Halcyonite2
{
    internal class RepeatingFistSkillBuilder : FistsSkillBuilder
    {
        public override string SkillName => "Repeating Fist";

        protected override void SetupSkillDriver(SkillDef skillDef, AISkillDriver skillDriver)
        {
            base.SetupSkillDriver(skillDef, skillDriver);

            skillDriver.skillSlot = SkillSlot.Utility;
            skillDriver.maxDistance = 120;
            skillDriver.selectionRequiresTargetLoS = false;
            skillDriver.selectionRequiresOnGround = true;
            skillDriver.activationRequiresAimConfirmation = false;
            skillDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            skillDriver.shouldSprint = true;
            skillDriver.maxUserHealthFraction = 0.75f; // TODO: DRY
        }

        protected override void SetupSkillDef(SkillDef skillDef, ManualLogSource logger)
        {
            base.SetupSkillDef(skillDef, logger);

            skillDef.activationState = Utils.AddEntityState<RepeatingFistSkillState>(logger);
            skillDef.baseMaxStock = 1;
            skillDef.baseRechargeInterval = 13f;
            skillDef.beginSkillCooldownOnSkillEnd = false;
            skillDef.cancelSprintingOnActivation = false;
            skillDef.rechargeStock = 1;
        }
    }
}

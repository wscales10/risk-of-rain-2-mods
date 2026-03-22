using BepInEx.Logging;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Skills;
using static PactOfPunishment.Waves.Stage1.Halcyonites.CustomWeaponStates;

namespace PactOfPunishment.Waves.Stage1.Halcyonites.Halcyonite3
{
    internal class LineOfFistsSkillBuilder : FistsSkillBuilder
    {
        public override string SkillName => "Line of Fists";

        protected override void SetupSkillDriver(SkillDef skillDef, AISkillDriver skillDriver)
        {
            base.SetupSkillDriver(skillDef, skillDriver);

            skillDriver.skillSlot = SkillSlot.Special;
            skillDriver.maxDistance = LineOfFistsSkillState.ZoneRadius * (LineOfFistsSkillState.ZonesToCreate + 1);
            skillDriver.selectionRequiresTargetLoS = false;
            skillDriver.selectionRequiresOnGround = true; // weird stuff happens if it uses this skill in the air
            skillDriver.activationRequiresAimConfirmation = true;
            skillDriver.movementType = AISkillDriver.MovementType.Stop;
            skillDriver.moveInputScale = 0;
            skillDriver.shouldSprint = false;
        }

        protected override void SetupSkillDef(SkillDef skillDef, ManualLogSource logger)
        {
            base.SetupSkillDef(skillDef, logger);

            skillDef.activationState = Utils.AddEntityState<LineOfFistsSkillState>(logger);
            skillDef.baseMaxStock = 2;
            skillDef.baseRechargeInterval = 20f;
            skillDef.cancelSprintingOnActivation = true;
            skillDef.rechargeStock = 2;
        }
    }
}

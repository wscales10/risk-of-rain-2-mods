using BepInEx.Logging;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Skills;

namespace PactOfPunishment.Waves.Stage1.Halcyonites
{
    internal abstract class FistsSkillBuilder : CustomSkillBuilder
    {
        protected override void SetupSkillDriver(SkillDef skillDef, AISkillDriver skillDriver)
        {
            base.SetupSkillDriver(skillDef, skillDriver);
            skillDriver.requiredSkill = skillDef;
            skillDriver.requireSkillReady = true;
            skillDriver.requireEquipmentReady = false;
            skillDriver.minDistance = 0;
            
            skillDriver.selectionRequiresTargetNonFlier = false;
            skillDriver.selectionRequiresAimTarget = false; // If require target, use target not aim target
            skillDriver.maxTimesSelected = -1;
            skillDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy; // This is correct.
            skillDriver.activationRequiresTargetLoS = false; // Selection can require LoS, but activation should not
            skillDriver.activationRequiresAimTargetLoS = false;
            skillDriver.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            skillDriver.ignoreNodeGraph = false;
            skillDriver.shouldFireEquipment = false;
            skillDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            skillDriver.resetCurrentEnemyOnNextDriverSelection = false;
            skillDriver.noRepeat = false;

            // newSkillDriver.nextHighPriorityOverride = ; newSkillDriver.enabled = true;
            skillDriver.useGUILayout = true;
        }

        protected override void SetupSkillDef(SkillDef skillDef, ManualLogSource logger)
        {
            base.SetupSkillDef(skillDef, logger);

            skillDef.activationStateMachineName = "Weapon";
            skillDef.canceledFromSprinting = false;
            skillDef.fullRestockOnAssign = true;
            skillDef.interruptPriority = EntityStates.InterruptPriority.Skill; // This basically just means it can't interrupt another skill.
            skillDef.isCombatSkill = true;
            skillDef.mustKeyPress = false;
            skillDef.requiredStock = 1;
            skillDef.stockToConsume = 1;
        }
    }
}

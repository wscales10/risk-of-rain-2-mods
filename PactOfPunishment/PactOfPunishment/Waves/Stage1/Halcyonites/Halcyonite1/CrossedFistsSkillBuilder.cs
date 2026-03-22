using BepInEx.Logging;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Skills;
using UnityEngine;
using static PactOfPunishment.Waves.Stage1.Halcyonites.CustomWeaponStates;

namespace PactOfPunishment.Waves.Stage1.Halcyonites.Halcyonite1
{
    internal class CrossedFistsSkillBuilder : FistsSkillBuilder
    {
        public override string SkillName => "Crossed Fists";

        protected override void SetupSkillDriver(SkillDef skillDef, AISkillDriver skillDriver)
        {
            base.SetupSkillDriver(skillDef, skillDriver);

            skillDriver.skillSlot = SkillSlot.Special;
            skillDriver.maxDistance = 120;
            skillDriver.selectionRequiresTargetLoS = false;
            skillDriver.selectionRequiresOnGround = true;
            skillDriver.activationRequiresAimConfirmation = false;
            skillDriver.movementType = AISkillDriver.MovementType.Stop;
            skillDriver.moveInputScale = 0;
            skillDriver.shouldSprint = false;
        }

        protected override void SetupSkillDef(SkillDef skillDef, ManualLogSource logger)
        {
            base.SetupSkillDef(skillDef, logger);

            Utils.OnLoad<GameObject>("RoR2/Base/Titan/TitanGoldPreFistProjectile.prefab", x => CrossedFistsSkillState.zoneProjectilePrefab = x);

            skillDef.activationState = Utils.AddEntityState<CrossedFistsSkillState>(logger);
            skillDef.baseMaxStock = 1;
            skillDef.baseRechargeInterval = 12f;
            skillDef.beginSkillCooldownOnSkillEnd = false;
            skillDef.cancelSprintingOnActivation = true;
            skillDef.rechargeStock = 1;
        }
    }
}
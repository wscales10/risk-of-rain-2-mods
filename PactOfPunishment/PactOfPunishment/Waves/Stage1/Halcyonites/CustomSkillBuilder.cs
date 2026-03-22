using BepInEx.Logging;
using R2API;
using RoR2.CharacterAI;
using RoR2.Skills;
using UnityEngine;

namespace PactOfPunishment.Waves.Stage1.Halcyonites
{
    internal abstract class CustomSkillBuilder
    {
        internal CustomBuiltSkill AddSkill(ManualLogSource logger)
        {
            var skillDef = ScriptableObject.CreateInstance<SkillDef>();
            this.SetupSkillDef(skillDef, logger);
            ContentAddition.AddSkillDef(skillDef);
            return new CustomBuiltSkill(skillDef, this.SetupSkillDriver);
        }

        protected virtual void SetupSkillDef(SkillDef skillDef, ManualLogSource logger)
        {
            skillDef.skillName = this.SkillName;
        }

        protected virtual void SetupSkillDriver(SkillDef skillDef, AISkillDriver skillDriver)
        {
            skillDriver.customName = skillDef.skillName;
        }

        public abstract string SkillName { get; }
    }
}
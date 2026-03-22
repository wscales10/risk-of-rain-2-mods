using HG;
using RoR2.CharacterAI;
using RoR2.Skills;
using System;

namespace PactOfPunishment.Waves.Stage1.Halcyonites
{
    internal class CustomBuiltSkill
    {
        private readonly Action<SkillDef, AISkillDriver> setupSkillDriver;

        public CustomBuiltSkill(SkillDef skillDef, Action<SkillDef, AISkillDriver> setupSkillDriver)
        {
            this.SkillDef = skillDef;
            this.setupSkillDriver = setupSkillDriver;
        }

        public SkillDef SkillDef { get; private set; }

        public void SetupSkillDriver(AISkillDriver skillDriver) => this.setupSkillDriver(this.SkillDef, skillDriver);

        public void InsertSkillDriver(BaseAI ai, int index)
        {
            var newSkillDriver = ai.gameObject.AddComponent<AISkillDriver>();
            this.SetupSkillDriver(newSkillDriver);
            ai.InsertSkillDriver(newSkillDriver, index);
        }
    }
}
using RoR2;

namespace PactOfPunishment.BugFixes
{
    public class ScorchWorm : Module
    {
        public override void Init()
        {
            GenericSkillHooks.IsSkillReady += this.GenericSkillHooks_IsSkillReady;
        }

        private void GenericSkillHooks_IsSkillReady(GenericSkill skill, ref bool isReady)
        {
            if (skill.characterBody.Is(DLC2Content.BodyPrefabs.ScorchlingBody) && skill.skillName == "Breach")
            {
                isReady &= skill.characterBody.TryGetComponent<ScorchlingController>(out var controller) && controller.isBurrowed;
            }
        }
    }
}
using RoR2;

namespace AssortedExperiments.Events
{
    public struct BossUseSkillContext
    {
        public CharacterBody OwnerBody { get; set; }

        public SkillSlot SkillSlot { get; set; }
    }
}
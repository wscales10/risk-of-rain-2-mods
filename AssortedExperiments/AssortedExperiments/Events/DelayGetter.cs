namespace AssortedExperiments.Events
{
    public static class DelayGetter
    {
        public static float GetDelay(BossUseSkillContext skillCtx, SummonGhostCard ghostCard)
        {
            return GetOwnerSkillDuration(skillCtx) - (GetSpawnTime(ghostCard) + GetGhostSkillDuration(ghostCard));
        }

        private static float GetSpawnTime(SummonGhostCard ghostCard)
        {
            return 0.9f;
        }

        private static float GetGhostSkillDuration(SummonGhostCard ghostCard)
        {
            return 2.75f;
        }

        private static float GetOwnerSkillDuration(BossUseSkillContext skillCtx)
        {
            return 4f;
        }
    }
}
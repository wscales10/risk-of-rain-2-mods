using R2API;
using RoR2;

namespace PactOfPunishment
{
    public static class Content
    {
        public static class Buffs
        {
            public static BuffDef ShieldedHealth;

            public static BuffDef MissedDeadline;
        }

        public static class Elites
        {
            public static EliteDef NerfedPoison;
        }

        public static class EliteTiers
        {
            public static CombatDirector.EliteTierDef NerfedPoisonTier;
        }

        public static class DamageTypes
        {
            public static DamageAPI.ModdedDamageType EnvironmentalHazard;
        }
    }
}
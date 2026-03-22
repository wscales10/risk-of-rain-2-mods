using R2API;
using RoR2;
using RoR2.Stats;

namespace PactOfPunishment
{
    public static class Content
    {
        public static class StatDefs
        {
            public static StatDef Heat;

            public static PerBodyStatDef PerBodyHeatEasy;

            public static PerBodyStatDef PerBodyHeatNormal;

            public static PerBodyStatDef PerBodyHeatHard;
        }

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

            public static DamageAPI.ModdedDamageType Stun1sBypassImmunity;
        }

        public static class MonsterSpawnDistances
        {
            public static DirectorCore.MonsterSpawnDistance WithinZone;
        }
    }
}
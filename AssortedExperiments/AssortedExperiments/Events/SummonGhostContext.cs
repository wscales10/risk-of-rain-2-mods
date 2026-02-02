using RoR2;
using System;

namespace AssortedExperiments.Events
{
    public struct SummonGhostContext
    {
        public CharacterBody OwnerBody { get; set; }

        public SpawnCard GhostSpawnCard { get; set; }

        public DirectorPlacementRule GhostPlacementRule { get; set; }

        public float StartingHealthFraction { get; set; }

        public float Lifespan { get; set; }

        public Func<SkillLocator, GenericSkill>[] BanSkills { get; set; }
    }
}
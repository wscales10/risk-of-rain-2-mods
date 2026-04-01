using RoR2;

namespace PactOfPunishment
{
    public class AddsSpawningArgs
    {
        public DirectorCardCategorySelection? MonsterCards { get; set; }

        public uint MaxSquadCount { get; set; }

        public float SpawnFrequency { get; set; }

        public float SpawnFrequencyVariation { get; set; }

        public float ExpectedDifficultyCoefficient { get; set; }

        public float InitialSpawnDelay { get; set; }
    }
}
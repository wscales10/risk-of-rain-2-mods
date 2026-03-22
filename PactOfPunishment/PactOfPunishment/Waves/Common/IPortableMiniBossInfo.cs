using RoR2;

namespace PactOfPunishment.Waves.Common
{
    public interface IPortableMiniBossInfo
    {
        CharacterBody BodyPrefab { get; }

        float RelativePowerLevel { get; }

        InfiniteTowerExplicitSpawnWaveController.SpawnInfo SpawnInfo { get; }
    }
}
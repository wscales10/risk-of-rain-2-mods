using PactOfPunishment.Conditions;
using RoR2;
using UnityEngine;

namespace PactOfPunishment.Waves.Infrastructure
{
    public interface IPermanentWaveSelectionDefinition : IWaveSelectionDefinition
    {
        IWaveSelectionDefinition? TryBuildForCustomWaveName(string key);
    }

    public interface IWaveSelectionDefinition
    {
        void ModifyWeightedSelection(WeightedSelection<GameObject?> weightedSelection, SimulacrumWaveDefinitions.Instance cache);

        UpgradeEncounterStrategy? GetUpgradeWaveStrategy(InfiniteTowerWaveController wave);
    }
}
using PactOfPunishment.Conditions;
using RoR2;
using System;
using System.Linq;
using UnityEngine;

namespace PactOfPunishment.Waves.Infrastructure
{
    public abstract class WaveSelectionDefinition : IPermanentWaveSelectionDefinition
    {
        private readonly (ISimulacrumWaveDefinition, float)[] customWaveDefinitions;

        protected WaveSelectionDefinition(params (ISimulacrumWaveDefinition, float)[] customWaveDefinitions)
        {
            this.customWaveDefinitions = customWaveDefinitions;
        }

        protected virtual float WeightOfOriginalSelection => 1;

        public virtual UpgradeWaveStrategy? GetUpgradeWaveStrategy(InfiniteTowerWaveController wave)
        {
            if (wave.TryGetComponent<UpgradeWaveBehavior>(out var behavior))
            {
                return behavior.upgradeStrategy;
            }
            else
            {
                return null;
            }
        }

        public virtual void ModifyWeightedSelection(WeightedSelection<GameObject?> weightedSelection, SimulacrumWaveDefinitions.Instance cache)
        {
            weightedSelection.AddChoicesWithRelativeWeight(this.WeightOfOriginalSelection, x => x, this.customWaveDefinitions.Select(x => (cache.Prefab(x.Item1), x.Item2)).ToArray());
        }

        public IWaveSelectionDefinition? TryBuildForCustomWaveName(string key)
        {
            return this.customWaveDefinitions.Where(x => string.Equals(x.Item1.Name, key, StringComparison.OrdinalIgnoreCase)).Select(x => new SingleWaveSelectionDefinition(x.Item1, this.GetUpgradeWaveStrategy)).SingleOrDefault();
        }
    }

    public class SingleWaveSelectionDefinition : IWaveSelectionDefinition
    {
        private readonly ISimulacrumWaveDefinition customWaveDefinition;

        private readonly Func<InfiniteTowerWaveController, UpgradeWaveStrategy?> getUpgradeWaveStrategy;

        public SingleWaveSelectionDefinition(ISimulacrumWaveDefinition customWaveDefinition, Func<InfiniteTowerWaveController, UpgradeWaveStrategy?> getUpgradeWaveStrategy)
        {
            this.customWaveDefinition = customWaveDefinition;
            this.getUpgradeWaveStrategy = getUpgradeWaveStrategy;
        }

        public UpgradeWaveStrategy? GetUpgradeWaveStrategy(InfiniteTowerWaveController wave)
        {
            return this.getUpgradeWaveStrategy(wave);
        }

        public void ModifyWeightedSelection(WeightedSelection<GameObject?> weightedSelection, SimulacrumWaveDefinitions.Instance cache)
        {
            weightedSelection.Clear();
            weightedSelection.AddChoice(cache.Prefab(this.customWaveDefinition), 1);
        }
    }
}
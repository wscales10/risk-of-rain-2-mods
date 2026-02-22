using PactOfPunishment.Conditions;
using PactOfPunishment.Waves.Stage1;
using PactOfPunishment.Waves.Stage1.Halcyonites;
using PactOfPunishment.Waves.Stage2;
using PactOfPunishment.Waves.Stage3;
using RoR2;
using UnityEngine;
using static PactOfPunishment.Conditions.MiddleManagement;

namespace PactOfPunishment.Waves.Infrastructure
{
    public class Wave5SelectionDefinition : ReplaceVanillaWaves
    {
        public Wave5SelectionDefinition(SimulacrumWaveDefinitions cache) : base((cache.Get<RunaldAndKjaro>(), 1), (cache.Get<SolusControlUnit>(), 1), (cache.Get<ImpOverlord>(), 1))
        {
        }
    }

    public class Wave10SelectionDefinition : ReplaceVanillaWaves
    {
        public Wave10SelectionDefinition(SimulacrumWaveDefinitions cache) : base((cache.Get<Halcyonite1>(), 1))
        {
        }
    }

    public class Wave15SelectionDefinition : WaveSelectionDefinition
    {
        public Wave15SelectionDefinition(SimulacrumWaveDefinitions cache) : base((cache.Get<WormAndDistributor>(), 1), (cache.Get<Projectilers>(), 1))
        {
        }

        public override void ModifyWeightedSelection(WeightedSelection<GameObject?> weightedSelection, SimulacrumWaveDefinitions.Instance cache)
        {
            weightedSelection.RemoveWhere(x => x?.GetComponent<InfiniteTowerWaveController>() is InfiniteTowerExplicitSpawnWaveController);
            base.ModifyWeightedSelection(weightedSelection, cache);
        }

        public override UpgradeWaveStrategy? GetUpgradeWaveStrategy(InfiniteTowerWaveController wave)
        {
            return base.GetUpgradeWaveStrategy(wave) ?? ScriptableObject.CreateInstance<MendingMiniMushrumUpgradeStrategy>();
        }
    }

    public class Wave20SelectionDefinition : ReplaceVanillaWaves
    {
        public Wave20SelectionDefinition(SimulacrumWaveDefinitions cache) : base((cache.Get<Summoner.Summoner>(), 1))
        {
        }
    }

    public class Wave25SelectionDefinition : ReplaceVanillaWaves
    {
        public Wave25SelectionDefinition(SimulacrumWaveDefinitions cache) : base((cache.Get<Mithrix>(), 1), (cache.Get<Summoner2>(), 1))
        {
        }
    }

    public class Wave30SelectionDefinition : ReplaceVanillaWaves
    {
        public Wave30SelectionDefinition(SimulacrumWaveDefinitions cache) : base((cache.Get<MithrixWithHalcyonite>(), 1))
        {
        }
    }

    public class Wave35SelectionDefinition : WaveSelectionDefinition
    {
        public override UpgradeWaveStrategy? GetUpgradeWaveStrategy(InfiniteTowerWaveController wave)
        {
            return null; // TODO: is this necessary?
        }

        public override void ModifyWeightedSelection(WeightedSelection<GameObject?> weightedSelection, SimulacrumWaveDefinitions.Instance cache)
        {
            weightedSelection.RemoveWhere(x => x?.GetComponent<InfiniteTowerWaveController>() is InfiniteTowerExplicitSpawnWaveController);
            base.ModifyWeightedSelection(weightedSelection, cache); // TODO: is this necessary?
        }
    }
}
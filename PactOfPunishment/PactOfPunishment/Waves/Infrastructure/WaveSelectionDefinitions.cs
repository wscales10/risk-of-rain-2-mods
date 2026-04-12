using HG;
using PactOfPunishment.Conditions;
using PactOfPunishment.Waves.Stage1;
using PactOfPunishment.Waves.Stage1.Halcyonites.Halcyonite1;
using PactOfPunishment.Waves.Stage1.Halcyonites.Halcyonite2;
using PactOfPunishment.Waves.Stage1.Halcyonites.Halcyonite3;
using PactOfPunishment.Waves.Stage2;
using PactOfPunishment.Waves.Stage3;
using PactOfPunishment.Waves.Stage4;
using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using static PactOfPunishment.Conditions.UpgradeMiniBosses;

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
        public Wave10SelectionDefinition(SimulacrumWaveDefinitions cache) : base((cache.Get<Halcyonite1>(), 1), (cache.Get<Halcyonite2>(), 1), (cache.Get<Halcyonite3>(), 1))
        {
        }
    }

    public class Wave15SelectionDefinition : WaveSelectionDefinition
    {
        private static readonly Dictionary<GameObject, GameObject> cloneCache = new Dictionary<GameObject, GameObject>();

        public Wave15SelectionDefinition(SimulacrumWaveDefinitions cache) : base((cache.Get<WormAndDistributor>(), 1), (cache.Get<Projectilers>(), 1))
        {
        }

        public override void ModifyWeightedSelection(WeightedSelection<GameObject?> weightedSelection, SimulacrumWaveDefinitions.Instance cache)
        {
            weightedSelection.RemoveWhere(x => x?.GetComponent<InfiniteTowerWaveController>() is InfiniteTowerExplicitSpawnWaveController);
            weightedSelection.Transform(choice =>
            {
                if (choice.value is null)
                {
                    return choice;
                }

                if (!cloneCache.TryGetValue(choice.value, out var clone))
                {
                    clone = PrefabAPI.InstantiateClone(choice.value, "Wave15" + choice.value.name);
                    var wave = clone.GetComponent<InfiniteTowerWaveController>();
                    wave.immediateCreditsFraction = 0.1f;
                    var waveModifiers = wave.EnsureComponent<SimulacrumCombatDirectorSpawnRateMultiplier>();
                    waveModifiers.TotalWaveCreditsMultiplier = 0.5f;
                    waveModifiers.WavePeriodSecondsMultiplier = 0.75f;

                    wave.EnsureComponent<SafeZoneRadiusCapper>().RadiusMultiplier = 0.75f;
                    cloneCache[choice.value] = clone;
                }

                return new WeightedSelection<GameObject?>.ChoiceInfo
                {
                    value = clone,
                    weight = choice.weight
                };
            });
            base.ModifyWeightedSelection(weightedSelection, cache);
        }

        public override UpgradeEncounterStrategy? GetUpgradeWaveStrategy(InfiniteTowerWaveController wave)
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
        public override UpgradeEncounterStrategy? GetUpgradeWaveStrategy(InfiniteTowerWaveController wave)
        {
            return null; // TODO: is this necessary?
        }

        public override void ModifyWeightedSelection(WeightedSelection<GameObject?> weightedSelection, SimulacrumWaveDefinitions.Instance cache)
        {
            weightedSelection.RemoveWhere(x => x?.GetComponent<InfiniteTowerWaveController>() is InfiniteTowerExplicitSpawnWaveController);
            base.ModifyWeightedSelection(weightedSelection, cache); // TODO: is this necessary?
        }
    }

    public class Wave40SelectionDefinition : ReplaceVanillaWaves
    {
        public Wave40SelectionDefinition(SimulacrumWaveDefinitions cache) : base((cache.Get<Aurelionite>(), 1), (cache.Get<BlazingElderLemurian>(), 1), (cache.Get<Gup>(), 1), (cache.Get<Invalidator>(), 1))
        {
        }
    }
}
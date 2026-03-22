using RoR2;
using System;
using System.Linq;
using UnityEngine;

namespace PactOfPunishment.Waves.Infrastructure
{
    public partial class SimulacrumWavesBehavior : MonoBehaviour
    {
        private InfiniteTowerRun run;

        private GameObject? defaultMithrix;

        private SimulacrumWaveDefinitions.Instance cache;

        private WaveSelectionDefinitionSelector waveSelectionDefinitionSelector;

        public string? WaveOverrideName { get; set; }

        internal IWaveSelectionDefinition? LastSelectedWaveSelectionDefinition { get; private set; }

        internal bool WasMithrixDefeatedEarlierInRun { get; set; }

        public bool TryOverrideWeightedSelection(InfiniteTowerWaveCategory self)
        {
            try
            {
                IWaveSelectionDefinition? waveSelectionDefinition = this.TryGetWaveSelectionDefinition(self);
                this.LastSelectedWaveSelectionDefinition = waveSelectionDefinition;

                if (!(waveSelectionDefinition is null))
                {
                    waveSelectionDefinition.ModifyWeightedSelection(self.weightedSelection, this.cache);
                    return true;
                }

                self.weightedSelection.RemoveWhere(x => x == this.defaultMithrix);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            return false;
        }

        private IWaveSelectionDefinition? TryGetWaveSelectionDefinition(InfiniteTowerWaveCategory self)
        {
            IWaveSelectionDefinition? waveSelectionDefinition;

            if (!string.IsNullOrEmpty(this.WaveOverrideName))
            {
                waveSelectionDefinition = this.waveSelectionDefinitionSelector.GetForCustomWaveName(this.WaveOverrideName);
                this.WaveOverrideName = null;

                if (waveSelectionDefinition != null)
                {
                    return waveSelectionDefinition;
                }
            }

            if (self.name != "BossWaveCategory")
            {
                return null;
            }

            return this.waveSelectionDefinitionSelector.GetForWaveIndex(this.run.waveIndex);
        }

        private void Awake()
        {
            this.run = this.GetComponent<InfiniteTowerRun>();
            this.cache = SimulacrumWavesModule.Instance.Cache.ForRun(this.run).Build();
            this.waveSelectionDefinitionSelector = new WaveSelectionDefinitionSelector(SimulacrumWavesModule.Instance.Cache);

            // Stage 3
            this.defaultMithrix = this.run.waveCategories.Single(x => x.name == "BossWaveCategory").wavePrefabs.Select(x => x.wavePrefab).Single(x => x.name == "InfiniteTowerWaveBossBrother");
        }
    }
}
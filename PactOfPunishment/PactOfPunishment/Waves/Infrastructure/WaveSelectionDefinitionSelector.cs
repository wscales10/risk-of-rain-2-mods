using System.Collections.Generic;
using System.Linq;

namespace PactOfPunishment.Waves.Infrastructure
{
    public class WaveSelectionDefinitionSelector
    {
        private readonly Dictionary<int, IPermanentWaveSelectionDefinition> dictionary;

        public WaveSelectionDefinitionSelector(SimulacrumWaveDefinitions cache)
        {
            this.dictionary = new Dictionary<int, IPermanentWaveSelectionDefinition>
            {
                { 5,  new Wave5SelectionDefinition(cache) },
                { 10, new Wave10SelectionDefinition(cache) },
                { 15, new Wave15SelectionDefinition(cache) },
                { 20, new Wave20SelectionDefinition(cache) },
                { 25, new Wave25SelectionDefinition(cache) },
                { 30, new Wave30SelectionDefinition(cache) },
                { 35, new Wave35SelectionDefinition() },
              //{ 40, new Wave40SelectionDefinition(cache) },
            };
        }

        public IWaveSelectionDefinition? GetForCustomWaveName(string key)
        {
            return this.dictionary.Values.Select(x => x.TryBuildForCustomWaveName(key)).SingleOrDefault(x => !(x is null));
        }

        public IWaveSelectionDefinition? GetForWaveIndex(int waveIndex)
        {
            int modifiedWaveIndex;

            if (waveIndex > 40)
            {
                modifiedWaveIndex = (waveIndex - 1) % 10 + 31;
            }
            else
            {
                modifiedWaveIndex = waveIndex;
            }

            if (this.dictionary.TryGetValue(modifiedWaveIndex, out var value))
            {
                return value;
            }

            return null;
        }
    }
}
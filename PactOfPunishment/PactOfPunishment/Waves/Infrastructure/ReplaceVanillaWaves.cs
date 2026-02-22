namespace PactOfPunishment.Waves.Infrastructure
{
    public abstract class ReplaceVanillaWaves : WaveSelectionDefinition
    {
        protected ReplaceVanillaWaves(params (ISimulacrumWaveDefinition, float)[] customWaveDefinitions) : base(customWaveDefinitions)
        {
        }

        protected override float WeightOfOriginalSelection => 0;
    }
}
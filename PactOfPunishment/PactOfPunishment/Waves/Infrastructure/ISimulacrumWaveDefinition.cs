using RoR2;
using UnityEngine;

namespace PactOfPunishment.Waves.Infrastructure
{
    public interface ISimulacrumWaveDefinition
    {
        string Name { get; }

        GameObject? MakeWavePrefab(Run run);
    }
}
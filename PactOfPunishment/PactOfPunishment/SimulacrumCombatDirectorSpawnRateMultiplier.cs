using RoR2;
using UnityEngine;

namespace PactOfPunishment
{
    [RequireComponent(typeof(CombatDirector))]
    public class SimulacrumCombatDirectorSpawnRateMultiplier : MonoBehaviour
    {
        public float TotalWaveCreditsMultiplier = 1;

        public float WavePeriodSecondsMultiplier = 1;

        public float CreditsPerAttemptMultiplier = 1;

        public float CreditGainRateMultiplier = 1;

        public float SpawnAttemptIntervalMultiplier => this.CreditsPerAttemptMultiplier * this.WavePeriodSecondsMultiplier / (this.TotalWaveCreditsMultiplier * this.CreditGainRateMultiplier);
    }
}
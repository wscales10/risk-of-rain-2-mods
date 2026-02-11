using RoR2;
using UnityEngine;

namespace PactOfPunishment.Conditions
{
    public class NullUpgradeStrategy : UpgradeWaveStrategy
    {
        public override void UpgradeWave(InfiniteTowerWaveController wave)
        {
            Debug.LogWarning($"No upgrade strategy for wave '{wave?.name}'");
        }
    }
}
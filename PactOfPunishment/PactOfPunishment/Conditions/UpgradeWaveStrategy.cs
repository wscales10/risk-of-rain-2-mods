using RoR2;
using UnityEngine;

namespace PactOfPunishment.Conditions
{
    public abstract class UpgradeWaveStrategy : ScriptableObject
    {
        public abstract void UpgradeWave(InfiniteTowerWaveController wave);
    }
}
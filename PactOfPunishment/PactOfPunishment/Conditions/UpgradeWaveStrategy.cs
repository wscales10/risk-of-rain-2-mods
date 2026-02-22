using RoR2;
using UnityEngine;

namespace PactOfPunishment.Conditions
{
    public abstract class UpgradeWaveStrategy : ScriptableObject
    {
        public abstract WaveUpgradeFilter WaveUpgradeFilter { get; }

        public virtual void PreInitialise(InfiniteTowerWaveController wave)
        {
        }

        public abstract void PostInitialise(InfiniteTowerWaveController wave);
    }
}
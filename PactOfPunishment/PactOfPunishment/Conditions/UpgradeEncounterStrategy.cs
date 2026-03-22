using RoR2;
using UnityEngine;

namespace PactOfPunishment.Conditions
{
    public abstract class UpgradeEncounterStrategy : ScriptableObject
    {
        public abstract WaveUpgradeFilter WaveUpgradeFilter { get; }

        public virtual void PreInitialise(EncounterContext ctx)
        {
        }

        public abstract void PostInitialise(EncounterContext ctx);
    }
}
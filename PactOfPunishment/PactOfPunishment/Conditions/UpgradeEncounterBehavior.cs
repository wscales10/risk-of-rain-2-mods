using System;
using UnityEngine;

namespace PactOfPunishment.Conditions
{
    public enum WaveUpgradeFilter
    {
        MiniBoss,

        MainBoss,
    }

    public class UpgradeEncounterBehavior : MonoBehaviour
    {
        public UpgradeEncounterStrategy? upgradeStrategy;
    }
}
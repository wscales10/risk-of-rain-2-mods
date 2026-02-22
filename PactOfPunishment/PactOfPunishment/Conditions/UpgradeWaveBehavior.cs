using System;
using UnityEngine;

namespace PactOfPunishment.Conditions
{
    public enum WaveUpgradeFilter
    {
        MiniBoss,

        MainBoss,
    }

    public class UpgradeWaveBehavior : MonoBehaviour
    {
        public UpgradeWaveStrategy? upgradeStrategy;
    }
}
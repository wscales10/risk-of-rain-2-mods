using System;
using UnityEngine;

namespace PactOfPunishment.Navigation
{
    public class FrustrationMonitor
    {
        private readonly Func<bool> getIsActive;

        private readonly float threshold;

        private readonly float decayRate;

        private readonly float increaseRateMultiplier;

        private readonly float minimumOverrideDuration;

        private float frustration;

        private bool isBuildingFrustration;

        private float dischargeTimer;

        public FrustrationMonitor(Func<bool> getIsActive, float threshold = 1f, float decayRate = 2f, float increaseRateMultiplier = 1f, float minimumOverrideDuration = 4f)
        {
            this.getIsActive = getIsActive;
            this.threshold = threshold;
            this.decayRate = decayRate;
            this.increaseRateMultiplier = increaseRateMultiplier;
            this.minimumOverrideDuration = minimumOverrideDuration;
        }

        public event Action<bool>? IsBuildingFrustrationChanged;

        public bool IsFrustrated => this.frustration > this.threshold;

        public bool IsBuildingFrustration
        {
            get => this.isBuildingFrustration;

            private set
            {
                if (this.isBuildingFrustration == value)
                {
                    return;
                }

                this.isBuildingFrustration = value;
                this.IsBuildingFrustrationChanged?.Invoke(value);
            }
        }

        public void Update(float expectedSpeed, float actualSpeed, float deltaTime)
        {
            float increaseRate = Mathf.Approximately(expectedSpeed, 0) ? 0 : Mathf.Clamp01(1 - actualSpeed / expectedSpeed);

            if (Mathf.Approximately(increaseRate, 0))
            {
                this.frustration = Mathf.Max(0, this.frustration - deltaTime * this.decayRate);
            }
            else if (this.getIsActive())
            {
                this.frustration += deltaTime * increaseRate * this.increaseRateMultiplier;
            }

            if (this.IsBuildingFrustration)
            {
                if (this.frustration > 1)
                {
                    this.dischargeTimer = this.minimumOverrideDuration;
                    this.IsBuildingFrustration = false;
                }
            }
            else
            {
                if (this.frustration < 0.5f && this.getIsActive())
                {
                    this.dischargeTimer -= deltaTime;
                }

                if (this.dischargeTimer < 0)
                {
                    this.IsBuildingFrustration = true;
                }
            }
        }

        public void Reset() => this.frustration = 0;
    }
}
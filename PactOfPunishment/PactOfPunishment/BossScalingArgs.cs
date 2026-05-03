using RoR2;
using UnityEngine;

namespace PactOfPunishment
{
    public interface IBossScalingArgs
    {
        float HpDivisor { get; }

        float DamageDivisor { get; }

        bool BoostHpByPlayerCount { get; }

        float ExpectedDifficultyCoefficient { get; }

        float IntendedBaseHpMultiplier { get; }

        float IntendedBaseDamageMultiplier { get; }

        float DeathRewardsMultiplier { get; }
    }

    public abstract class BossScalingArgs : IBossScalingArgs
    {
        protected BossScalingArgs(int waveIndex)
        {
            this.ExpectedDifficultyCoefficient = Utils.EstimateDifficultyCoefficient(waveIndex);
        }

        public abstract float HpDivisor { get; }

        public abstract float DamageDivisor { get; }

        public abstract bool BoostHpByPlayerCount { get; }

        public float ExpectedDifficultyCoefficient { get; }

        public abstract float IntendedBaseHpMultiplier { get; }

        public abstract float IntendedBaseDamageMultiplier { get; }

        public bool ScaleDeathRewards { get; set; } = true;

        public float DeathRewardsMultiplierMultiplier { get; set; } = 1f;

        public float DeathRewardsMultiplier => this.ScaleDeathRewards ? this.IntendedBaseHpMultiplier * this.DeathRewardsMultiplierMultiplier : 1f;
    }

    public class BossScalingArgs1 : BossScalingArgs
    {
        public BossScalingArgs1(float hpDivisor, float damageDivisor, bool boostHpByPlayerCount, int waveIndex) : base(waveIndex)
        {
            this.HpDivisor = hpDivisor;
            this.DamageDivisor = damageDivisor;
            this.BoostHpByPlayerCount = boostHpByPlayerCount;
        }

        public override float HpDivisor { get; }

        public override float DamageDivisor { get; }

        public override bool BoostHpByPlayerCount { get; }

        public override float IntendedBaseHpMultiplier => 1 + this.ExpectedDifficultyCoefficient / this.HpDivisor;

        public override float IntendedBaseDamageMultiplier => 1 + this.ExpectedDifficultyCoefficient / this.DamageDivisor;
    }

    public class BossScalingArgs2 : BossScalingArgs
    {
        public BossScalingArgs2(float intendedBaseHpMultiplier, float intendedBaseDamageMultiplier, bool boostHpByPlayerCount, int waveIndex) : base(waveIndex)
        {
            this.IntendedBaseHpMultiplier = intendedBaseHpMultiplier;
            this.IntendedBaseDamageMultiplier = intendedBaseDamageMultiplier;
            this.BoostHpByPlayerCount = boostHpByPlayerCount;
        }

        public override float HpDivisor => this.ExpectedDifficultyCoefficient / (this.IntendedBaseHpMultiplier - 1);

        public override float DamageDivisor => this.ExpectedDifficultyCoefficient / (this.IntendedBaseDamageMultiplier - 1);

        public override bool BoostHpByPlayerCount { get; }

        public override float IntendedBaseHpMultiplier { get; }

        public override float IntendedBaseDamageMultiplier { get; }
    }
}
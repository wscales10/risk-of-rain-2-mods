using RoR2;
using System;
using UnityEngine;

namespace PactOfPunishment.Conditions
{
    [ModuleDependency(typeof(ModifyHealthGain))]
    public sealed class ReduceAllyHealing : DefaultConditionDef, IModifyHealthGain
    {
        private const float healingReductionPerRank = 0.25f;

        private static float barrierReductionScale = 0f;

        private ReduceAllyHealing()
        {
        }

        public static ReduceAllyHealing Instance { get; } = new ReduceAllyHealing();

        public override int MaxRank => 4;

        public override string Description => string.Format(base.Description, Utils.Percent(healingReductionPerRank));

        public override void Init()
        {
        }

        public void ModifyHealthGain(HealthGainModificationArgs args)
        {
            if (args.HealthComponent.body.teamComponent.teamIndex != TeamIndex.Player)
            {
                return;
            }

            var reductionScale = GetReductionScaleFromHealthGainType(args.HealthGainType);

            int rank = this.GetRank(args.HealthComponent);

            if (rank > 0)
            {
                args.HealthGainMultiplier *= Mathf.Max(0, 1 - reductionScale * healingReductionPerRank * rank);
            }
        }

        private static float GetReductionScaleFromHealthGainType(HealthGainType healthGainType)
        {
            return healthGainType switch
            {
                HealthGainType.Heal => 1,
                HealthGainType.Shield => 1,
                HealthGainType.Barrier => barrierReductionScale,
                _ => throw new ArgumentOutOfRangeException(nameof(healthGainType), healthGainType, null),
            };
        }
    }
}
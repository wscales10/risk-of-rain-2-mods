using R2API;
using RoR2;

namespace PactOfPunishment.Conditions
{
    public sealed class ForcedOvertime : DefaultConditionDef
    {
        public override int MaxRank => 2;

        public override int HeatPerRank => 3;

        public bool ReduceEnemyCooldowns { get; set; } = true;

        public override void Init()
        {
            RecalculateStatsAPI.GetStatCoefficients += this.RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (Utils.IsFoe(sender))
            {
                float multiplier = 1 + 0.2f * this.GetRank(sender);
                args.moveSpeedTotalMult *= multiplier;
                args.attackSpeedTotalMult *= multiplier;

                if (this.ReduceEnemyCooldowns)
                {
                    args.allSkills.cooldownMultiplier /= multiplier;
                }
            }
        }
    }
}
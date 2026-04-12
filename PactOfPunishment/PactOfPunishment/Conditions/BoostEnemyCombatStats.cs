using R2API;
using RoR2;

namespace PactOfPunishment.Conditions
{
    public sealed class BoostEnemyCombatStats : DefaultConditionDef
    {
        public override int MaxRank => 2;

        public override int HeatPerRank => 3;
        
        private const float combatStatIncreasePerRank = 0.2f;

        public override string Description => string.Format(base.Description, Utils.Percent(combatStatIncreasePerRank));

        public bool ReduceEnemyCooldowns { get; set; } = true;

        public override void Init()
        {
            RecalculateStatsAPI.GetStatCoefficients += this.RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (Utils.IsFoe(sender))
            {
                float multiplier = 1 + combatStatIncreasePerRank * this.GetRank(sender);
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
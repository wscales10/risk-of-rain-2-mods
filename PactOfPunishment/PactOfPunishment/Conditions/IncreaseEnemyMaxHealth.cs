using R2API;
using RoR2;

namespace PactOfPunishment.Conditions
{
    public sealed class IncreaseEnemyMaxHealth : DefaultConditionDef
    {
        public override int MaxRank => 2;

        private const float healthIncreasePerRank = 0.15f;

        public override string Description => string.Format(base.Description, Utils.Percent(healthIncreasePerRank));

        public override void Init()
        {
            RecalculateStatsAPI.GetStatCoefficients += this.RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (Utils.IsFoe(sender))
            {
                args.healthTotalMult *= 1 + healthIncreasePerRank * this.GetRank(sender);
            }
        }
    }
}
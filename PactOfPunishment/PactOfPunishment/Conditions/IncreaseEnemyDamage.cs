using R2API;
using RoR2;

namespace PactOfPunishment.Conditions
{
    public sealed class IncreaseEnemyDamage : DefaultConditionDef
    {
        public override int MaxRank => 5;

        private const float damageIncreasePerRank = 0.2f;

        public override string Description => string.Format(base.Description, Utils.Percent(damageIncreasePerRank));

        public override void Init()
        {
            RecalculateStatsAPI.GetStatCoefficients += this.RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (Utils.IsFoe(sender))
            {
                args.damageTotalMult *= 1 + 0.2f * this.GetRank(sender);
            }
        }
    }
}
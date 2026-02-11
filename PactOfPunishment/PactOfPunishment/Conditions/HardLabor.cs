using R2API;
using RoR2;

namespace PactOfPunishment.Conditions
{
    public sealed class HardLabor : DefaultConditionDef
    {
        public override int MaxRank => 5;

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
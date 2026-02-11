using R2API;
using RoR2;

namespace PactOfPunishment.Conditions
{
    public sealed class CalisthenicsProgram : DefaultConditionDef
    {
        public override int MaxRank => 2;

        public override void Init()
        {
            RecalculateStatsAPI.GetStatCoefficients += this.RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (Utils.IsFoe(sender))
            {
                args.healthTotalMult *= 1 + 0.15f * this.GetRank(sender);
            }
        }
    }
}
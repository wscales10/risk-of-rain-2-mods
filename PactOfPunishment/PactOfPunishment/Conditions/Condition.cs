using System;
using System.Linq;

namespace PactOfPunishment.Conditions
{
    public class Condition
    {
        public Condition(IConditionDef conditionDef, int rank)
        {
            this.ConditionDef = conditionDef ?? throw new ArgumentNullException(nameof(conditionDef));

            if (rank < 0 || rank > conditionDef.MaxRank)
            {
                throw new ArgumentOutOfRangeException(nameof(rank));
            }

            this.Rank = rank;
        }

        public IConditionDef ConditionDef { get; }

        public int Rank { get; }

        public int TotalHeat => this.Rank < 1 ? 0 : Enumerable.Range(1, this.Rank).Select(this.ConditionDef.GetHeatForRank).Sum();
    }
}
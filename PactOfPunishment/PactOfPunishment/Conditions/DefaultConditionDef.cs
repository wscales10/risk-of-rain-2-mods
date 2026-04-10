namespace PactOfPunishment.Conditions
{
    public abstract class DefaultConditionDef : ConditionDef
    {
        public virtual int HeatPerRank => 1;

        public override int GetHeatForRank(int rank) => this.HeatPerRank;
    }
}
namespace PactOfPunishment.Conditions
{
    public abstract class DefaultConditionDef : ConditionDef
    {
        public virtual int HeatPerRank => 1;

        public override int GetHeatForRank(int rank) => this.HeatPerRank;
    }

    public sealed class ExtremeMeasures : ConditionDef
    {
        public override int MaxRank => 4;

        public override int GetHeatForRank(int rank) => rank;
    }

    public sealed class HeightenedSecurity : DefaultConditionDef
    {
        public override int MaxRank => 1;
    }

    public sealed class RoutineInspection : DefaultConditionDef
    {
        public override int MaxRank => 4;

        public override int HeatPerRank => 2;
    }
}
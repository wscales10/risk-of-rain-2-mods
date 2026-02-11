namespace PactOfPunishment.Conditions
{
    public interface IConditionDef
    {
        int MaxRank { get; }

        int GetHeatForRank(int rank);

        void Init();
    }
}
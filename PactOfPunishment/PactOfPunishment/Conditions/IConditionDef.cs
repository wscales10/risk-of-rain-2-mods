namespace PactOfPunishment.Conditions
{
    public interface IConditionDef
    {
        int MaxRank { get; }
        string Name { get; }
        string Description { get; }

        int GetHeatForRank(int rank);

        void Init();
    }
}
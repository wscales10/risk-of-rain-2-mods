namespace Rules.RuleTypes.Interfaces
{
    public interface IBucket<TContext, TOut> : IRule<TContext, TOut>
    {
        TOut Output { get; }
    }
}
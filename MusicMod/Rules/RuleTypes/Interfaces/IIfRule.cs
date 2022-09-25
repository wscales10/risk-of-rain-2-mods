using Patterns;

namespace Rules.RuleTypes.Interfaces
{
    public interface IIfRule<TContext, TOut> : IRule<TContext, TOut>
    {
        IPattern<TContext> Pattern { get; }

        IRule<TContext, TOut> ThenRule { get; }

        IRule<TContext, TOut> ElseRule { get; }
    }
}
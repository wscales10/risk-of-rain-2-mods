using Patterns;

namespace Rules.RuleTypes.Interfaces
{
    public interface IIfRule<TContext> : IRule<TContext>
    {
        IPattern<TContext> Pattern { get; }

        IRule<TContext> ThenRule { get; }

        IRule<TContext> ElseRule { get; }
    }
}
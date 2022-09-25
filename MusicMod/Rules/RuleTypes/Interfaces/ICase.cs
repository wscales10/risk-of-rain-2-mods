using Patterns;
using Rules.RuleTypes.Mutable;
using System.Collections.Generic;

namespace Rules.RuleTypes.Interfaces
{
    public interface ICase<T, TContext, TOut>
    {
        IPattern<TContext> WherePattern { get; }

        IEnumerable<T> Arr { get; }

        IRule<TContext, TOut> Output { get; }

        string Name { get; }
    }

    public interface ICaseGetter<T, TContext, TOut>
    {
        IEnumerable<RuleCase<T, TContext, TOut>> GetCases();
    }
}
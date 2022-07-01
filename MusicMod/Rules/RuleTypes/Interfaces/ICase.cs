using Patterns;
using Rules.RuleTypes.Mutable;
using System.Collections.Generic;

namespace Rules.RuleTypes.Interfaces
{
    public interface ICase<T, TContext>
    {
        IPattern<TContext> WherePattern { get; }

        IEnumerable<T> Arr { get; }

        IRule<TContext> Output { get; }

        string Name { get; }
    }

    public interface ICaseGetter<T, TContext>
    {
        IEnumerable<RuleCase<T, TContext>> GetCases();
    }
}
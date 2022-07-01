using System.Collections.Generic;

namespace Rules.RuleTypes.Interfaces
{
    public interface IMultiRule<T, TContext>
    {
        IEnumerable<(T expectedValue, IRule<TContext> rule)> Pairs { get; }

        string PropertyName { get; }
    }
}
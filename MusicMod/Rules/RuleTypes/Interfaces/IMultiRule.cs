using System.Collections.Generic;

namespace Rules.RuleTypes.Interfaces
{
    public interface IMultiRule<T, TContext, TOut>
    {
        IEnumerable<(T expectedValue, IRule<TContext, TOut> rule)> Pairs { get; }

        string PropertyName { get; }
    }
}
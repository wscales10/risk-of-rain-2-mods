using System.Collections.Generic;

namespace Rules.RuleTypes.Interfaces
{
    public interface IArrayRule<TContext, TOut> : IRule<TContext, TOut>
    {
        IEnumerable<IRule<TContext, TOut>> Rules { get; }
    }
}
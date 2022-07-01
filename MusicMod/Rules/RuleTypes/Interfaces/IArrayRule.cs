using System.Collections.Generic;

namespace Rules.RuleTypes.Interfaces
{
    public interface IArrayRule<TContext> : IRule<TContext>
    {
        IEnumerable<IRule<TContext>> Rules { get; }
    }
}
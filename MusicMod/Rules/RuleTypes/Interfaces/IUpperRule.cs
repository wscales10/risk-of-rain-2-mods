using Rules.RuleTypes.Mutable;
using System.Collections.Generic;

namespace Rules.RuleTypes.Interfaces
{
    internal interface IUpperRule<TContext, TOut> : IRule<TContext, TOut>
    {
        IEnumerable<Rule<TContext, TOut>> GetRules(TContext c);
    }
}
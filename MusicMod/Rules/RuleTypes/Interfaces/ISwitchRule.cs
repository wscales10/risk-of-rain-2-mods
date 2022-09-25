using Patterns;
using Patterns.Patterns;
using System.Collections.Generic;

namespace Rules.RuleTypes.Interfaces
{
    public interface ISwitchRule<TContext, TOut> : IRule<TContext, TOut>
    {
        PropertyInfo PropertyInfo { get; }

        IEnumerable<ICase<IPattern, TContext, TOut>> Cases { get; }

        IRule<TContext, TOut> DefaultRule { get; }
    }
}
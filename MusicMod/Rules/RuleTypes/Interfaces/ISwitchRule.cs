using Patterns;
using Patterns.Patterns;
using System.Collections.Generic;

namespace Rules.RuleTypes.Interfaces
{
    public interface ISwitchRule<TContext> : IRule<TContext>
    {
        PropertyInfo PropertyInfo { get; }

        IEnumerable<ICase<IPattern, TContext>> Cases { get; }

        IRule<TContext> DefaultRule { get; }
    }
}
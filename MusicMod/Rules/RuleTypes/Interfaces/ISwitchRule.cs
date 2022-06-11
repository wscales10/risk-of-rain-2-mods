using Patterns;
using Patterns.Patterns;
using System.Collections.Generic;

namespace Rules.RuleTypes.Interfaces
{
    public interface ISwitchRule : IRule
    {
        PropertyInfo PropertyInfo { get; }

        IEnumerable<ICase<IPattern>> Cases { get; }

        IRule DefaultRule { get; }
    }
}
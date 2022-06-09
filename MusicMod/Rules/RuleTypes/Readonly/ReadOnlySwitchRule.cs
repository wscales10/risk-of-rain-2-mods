using Patterns;
using Patterns.Patterns;
using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Mutable;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Utils;

namespace Rules.RuleTypes.Readonly
{
    public class ReadOnlySwitchRule : ReadOnlyRule<StaticSwitchRule>, ISwitchRule
    {
        public ReadOnlySwitchRule(StaticSwitchRule staticSwitchRule) : base(staticSwitchRule)
        {
            PropertyInfo = staticSwitchRule.PropertyInfo;
            DefaultRule = staticSwitchRule.DefaultRule?.ToReadOnly();
            Cases = staticSwitchRule.Cases.Select(c => c.ToReadOnly()).ToReadOnlyCollection();
        }

        public PropertyInfo PropertyInfo { get; }

        public ReadOnlyCollection<ReadOnlyCase<IPattern>> Cases { get; }

        public IRule DefaultRule { get; }

        IEnumerable<ICase<IPattern>> ISwitchRule.Cases => Cases;
    }
}
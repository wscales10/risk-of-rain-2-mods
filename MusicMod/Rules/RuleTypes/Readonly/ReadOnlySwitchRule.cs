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
    public class ReadOnlySwitchRule<TContext, TOut> : ReadOnlyUpperRule<StaticSwitchRule<TContext, TOut>, TContext, TOut>, ISwitchRule<TContext, TOut>
    {
        public ReadOnlySwitchRule(StaticSwitchRule<TContext, TOut> staticSwitchRule) : base(staticSwitchRule)
        {
            PropertyInfo = staticSwitchRule.PropertyInfo;
            DefaultRule = staticSwitchRule.DefaultRule?.ToReadOnly();
            Cases = staticSwitchRule.Cases.Select(c => c.ToReadOnly()).ToReadOnlyCollection();
        }

        public PropertyInfo PropertyInfo { get; }

        public ReadOnlyCollection<ReadOnlyCase<IPattern, TContext, TOut>> Cases { get; }

        public IRule<TContext, TOut> DefaultRule { get; }

        IEnumerable<ICase<IPattern, TContext, TOut>> ISwitchRule<TContext, TOut>.Cases => Cases;
    }
}
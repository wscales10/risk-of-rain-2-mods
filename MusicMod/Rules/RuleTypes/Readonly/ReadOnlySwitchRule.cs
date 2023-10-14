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
        public ReadOnlySwitchRule(StaticSwitchRule<TContext, TOut> staticSwitchRule, RuleParser<TContext, TOut> ruleParser) : base(staticSwitchRule, ruleParser)
        {
            DefaultRule = mutable.DefaultRule?.ToReadOnly(ruleParser);
            Cases = mutable.Cases.Select(c => c.ToReadOnly(ruleParser)).ToReadOnlyCollection();
        }

        public PropertyInfo PropertyInfo => mutable.PropertyInfo;

        public ReadOnlyCollection<ReadOnlyCase<IPattern, TContext, TOut>> Cases { get; }

        public IRule<TContext, TOut> DefaultRule { get; }

        IEnumerable<ICase<IPattern, TContext, TOut>> ISwitchRule<TContext, TOut>.Cases => Cases;

        public override IEnumerable<(string, IRule<TContext, TOut>)> Children => StaticSwitchRule.GetChildren(this);
    }
}
using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Mutable;
using System.Collections.Generic;
using Utils;

namespace Rules.RuleTypes.Readonly
{
    public class ReadOnlyArrayRule<TContext, TOut> : ReadOnlyUpperRule<ArrayRule<TContext, TOut>, TContext, TOut>, IArrayRule<TContext, TOut>
    {
        public ReadOnlyArrayRule(ArrayRule<TContext, TOut> arrayRule, RuleParser<TContext, TOut> ruleParser) : base(arrayRule, ruleParser)
        {
        }

        public bool IsRandom => mutable.IsRandom;

        IEnumerable<IRule<TContext, TOut>> IArrayRule<TContext, TOut>.Rules => mutable.Rules.ToReadOnlyCollection();

        public override IEnumerable<(string, IRule<TContext, TOut>)> Children => ArrayRule.GetChildren(this);
    }
}
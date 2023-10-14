using Patterns;
using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Mutable;
using System.Collections.Generic;

namespace Rules.RuleTypes.Readonly
{
    public class ReadOnlyIfRule<TContext, TOut> : ReadOnlyUpperRule<IfRule<TContext, TOut>, TContext, TOut>, IIfRule<TContext, TOut>
    {
        public ReadOnlyIfRule(IfRule<TContext, TOut> ifRule, RuleParser<TContext, TOut> ruleParser) : base(ifRule, ruleParser)
        {
            Pattern = new ReadOnlyPattern<TContext>(mutable.Pattern);
            ThenRule = mutable.ThenRule.ToReadOnly(ruleParser);
            ElseRule = mutable.ElseRule?.ToReadOnly(ruleParser);
        }

        public IPattern<TContext> Pattern { get; }

        public IRule<TContext, TOut> ThenRule { get; }

        public IRule<TContext, TOut> ElseRule { get; }

        public override IEnumerable<(string, IRule<TContext, TOut>)> Children => IfRule.GetChildren(this);
    }
}
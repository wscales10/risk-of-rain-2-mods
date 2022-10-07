using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Mutable;
using System.Collections.Generic;

namespace Rules.RuleTypes.Readonly
{
    public abstract class ReadOnlyUpperRule<T, TContext, TOut> : ReadOnlyRule<T, TContext, TOut>, IUpperRule<TContext, TOut>
        where T : UpperRule<TContext, TOut>
    {
        protected ReadOnlyUpperRule(T mutable, RuleParser<TContext, TOut> ruleParser) : base(mutable, ruleParser)
        {
        }

        public IEnumerable<Rule<TContext, TOut>> GetRules(TContext c) => mutable.GetRules(c);

        public sealed override TrackedResponse<TContext, TOut> GetBucket(TContext c) => UpperRule<TContext, TOut>.GetBucket(this, c);
    }
}
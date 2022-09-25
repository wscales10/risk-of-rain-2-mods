using Rules.RuleTypes.Interfaces;
using System.Collections.Generic;

namespace Rules.RuleTypes.Mutable
{
    public abstract class UpperRule<TContext, TOut> : Rule<TContext, TOut>, IUpperRule<TContext, TOut>
    {
        public abstract IEnumerable<Rule<TContext, TOut>> GetRules(TContext c);

        public sealed override TrackedResponse<TContext, TOut> GetBucket(TContext c) => GetBucket(this, c);

        internal static TrackedResponse<TContext, TOut> GetBucket(IUpperRule<TContext, TOut> parentRule, TContext c)
        {
            foreach (var childRule in parentRule.GetRules(c))
            {
                var output = childRule?.GetBucket(c);

                if (!(output?.Bucket is null))
                {
                    return output.With(parentRule);
                }
            }

            return new TrackedResponse<TContext, TOut>(null);
        }
    }
}
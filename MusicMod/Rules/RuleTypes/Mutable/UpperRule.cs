using System.Collections.Generic;

namespace Rules.RuleTypes.Mutable
{
    public abstract class UpperRule<TContext> : Rule<TContext>
    {
        public abstract IEnumerable<Rule<TContext>> GetRules(TContext c);

        public sealed override TrackedResponse<TContext> GetBucket(TContext c)
        {
            foreach (var rule in GetRules(c))
            {
                var output = rule?.GetBucket(c);

                if (!(output?.Bucket is null))
                {
                    return output.With(this);
                }
            }

            return new TrackedResponse<TContext>(null);
        }
    }
}
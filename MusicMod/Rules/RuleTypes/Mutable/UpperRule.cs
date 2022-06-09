using MyRoR2;
using System.Collections.Generic;

namespace Rules.RuleTypes.Mutable
{
    public abstract class UpperRule : Rule
    {
        public abstract IEnumerable<Rule> GetRules(Context c);

        public sealed override TrackedResponse GetBucket(Context c)
        {
            foreach (var rule in GetRules(c))
            {
                var output = rule?.GetBucket(c);

                if (!(output?.Bucket is null))
                {
                    return output.With(this);
                }
            }

            return new TrackedResponse(null);
        }
    }
}
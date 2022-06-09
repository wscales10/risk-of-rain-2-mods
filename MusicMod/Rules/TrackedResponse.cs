using Rules.RuleTypes.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Rules
{
    public class TrackedResponse
    {
        public TrackedResponse(IBucket bucket) : this(bucket, Enumerable.Empty<IRule>())
        {
        }

        private TrackedResponse(IBucket bucket, IEnumerable<IRule> rules)
        {
            Rules = new Stack<IRule>(rules);
            Rules.Push(Bucket = bucket);
        }

        public IBucket Bucket { get; }

        public Stack<IRule> Rules { get; }

        public TrackedResponse With(IRule rule)
        {
            Rules.Push(rule);
            return this;
        }

        public TrackedResponse ToReadOnly() => new TrackedResponse((IBucket)Bucket.ToReadOnly(), Rules.Select(r => r.ToReadOnly()));
    }
}
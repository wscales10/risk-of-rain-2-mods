using Rules.RuleTypes.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Utils;

namespace Rules
{
    public static class TrackedResponse
    {
        public static TrackedResponse<TContext> Create<TContext>(IBucket<TContext> bucket) => new TrackedResponse<TContext>(bucket);
    }

    public class TrackedResponse<TContext>
    {
        public TrackedResponse(IBucket<TContext> bucket) : this(bucket, Enumerable.Empty<IRule<TContext>>())
        {
        }

        private TrackedResponse(IBucket<TContext> bucket, IEnumerable<IRule<TContext>> rules)
        {
            Rules = new Stack<IRule<TContext>>(rules);
            Rules.Push(Bucket = bucket);
        }

        public IBucket<TContext> Bucket { get; }

        public Stack<IRule<TContext>> Rules { get; }

        public TrackedResponse<TContext> With(IRule<TContext> rule)
        {
            Rules.Push(rule);
            return this;
        }

        public void LogRules()
        {
            int i = 0;

            foreach (var rule in Rules)
            {
                this.Log(new string(' ', i) + '-' + rule);
                i++;
            }
        }

        public TrackedResponse<TContext> ToReadOnly() => new TrackedResponse<TContext>((IBucket<TContext>)Bucket.ToReadOnly(), Rules.Select(r => r.ToReadOnly()));
    }
}
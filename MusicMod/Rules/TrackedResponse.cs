using Rules.RuleTypes.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Utils;

namespace Rules
{
    public static class TrackedResponse
    {
        public static TrackedResponse<TContext, TOut> Create<TContext, TOut>(IBucket<TContext, TOut> bucket) => new TrackedResponse<TContext, TOut>(bucket);
    }

    public class TrackedResponse<TContext, TOut>
    {
        public TrackedResponse(IBucket<TContext, TOut> bucket) : this(bucket, Enumerable.Empty<IRule<TContext, TOut>>())
        {
        }

        private TrackedResponse(IBucket<TContext, TOut> bucket, IEnumerable<IRule<TContext, TOut>> rules)
        {
            Rules = new Stack<IRule<TContext, TOut>>(rules);
            Rules.Push(Bucket = bucket);
        }

        public IBucket<TContext, TOut> Bucket { get; }

        public Stack<IRule<TContext, TOut>> Rules { get; }

        public TrackedResponse<TContext, TOut> With(IRule<TContext, TOut> rule)
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

        public TrackedResponse<TContext, TOut> ToReadOnly(RuleParser<TContext, TOut> ruleParser)
        {
            return new TrackedResponse<TContext, TOut>((IBucket<TContext, TOut>)Bucket.ToReadOnly(ruleParser), Rules.Select(r => r.ToReadOnly(ruleParser)));
        }
    }
}
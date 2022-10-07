using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Mutable;
using Utils;

namespace Rules.RuleTypes.Readonly
{
    public class ReadOnlyBucket<TContext, TOut> : ReadOnlyRule<Bucket<TContext, TOut>, TContext, TOut>, IBucket<TContext, TOut>
    {
        private readonly TrackedResponse<TContext, TOut> response;

        public ReadOnlyBucket(Bucket<TContext, TOut> bucket, RuleParser<TContext, TOut> ruleParser) : base(bucket, ruleParser)
        {
            var output = bucket.Output;
            Output = output is IMutable<TOut> mutable ? mutable.ToReadOnly() : output;
            response = TrackedResponse.Create(this).ToReadOnly(ruleParser);
        }

        public TOut Output { get; }

        TOut IBucket<TContext, TOut>.Output => Output;

        public override TrackedResponse<TContext, TOut> GetBucket(TContext c) => response;
    }
}
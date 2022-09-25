using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Mutable;
using Utils;

namespace Rules.RuleTypes.Readonly
{
    public class ReadOnlyBucket<TContext, TOut> : ReadOnlyRule<Bucket<TContext, TOut>, TContext, TOut>, IBucket<TContext, TOut>
    {
        public ReadOnlyBucket(Bucket<TContext, TOut> bucket) : base(bucket)
        {
            var output = bucket.Output;
            Output = output is IMutable<TOut> mutable ? mutable.ToReadOnly() : output;
        }

        public TOut Output { get; }

        TOut IBucket<TContext, TOut>.Output => Output;

        public override TrackedResponse<TContext, TOut> GetBucket(TContext c) => TrackedResponse.Create(this).ToReadOnly();
    }
}
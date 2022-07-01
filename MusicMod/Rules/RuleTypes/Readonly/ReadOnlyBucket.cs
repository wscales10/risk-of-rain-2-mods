using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Mutable;
using Spotify.Commands;

namespace Rules.RuleTypes.Readonly
{
    public class ReadOnlyBucket<TContext> : ReadOnlyRule<Bucket<TContext>, TContext>, IBucket<TContext>
    {
        public ReadOnlyBucket(Bucket<TContext> bucket) : base(bucket)
        {
            Commands = bucket.Commands.ToReadOnly();
        }

        public ReadOnlyCommandList Commands { get; }

        ICommandList IBucket<TContext>.Commands => Commands;
    }
}
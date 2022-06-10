using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Mutable;
using Spotify.Commands.Interfaces;
using Spotify.Commands.Mutable;

namespace Rules.RuleTypes.Readonly
{
    public class ReadOnlyBucket : ReadOnlyRule<Bucket>, IBucket
    {
        public ReadOnlyBucket(Bucket bucket) : base(bucket)
        {
            Commands = bucket.Commands.ToReadOnly();
        }

        public ReadOnlyCommandList Commands { get; }

        ICommandList IBucket.Commands => Commands;
    }
}
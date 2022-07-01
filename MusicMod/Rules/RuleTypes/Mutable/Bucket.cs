using Patterns;
using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Readonly;
using Spotify;
using Spotify.Commands;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Rules.RuleTypes.Mutable
{
    public static class Bucket
    { }

    public class Bucket<TContext> : Rule<TContext>, IBucket<TContext>
    {
        public Bucket(params Command[] commands)
        {
            Commands = commands;
        }

        public Bucket(IList<Command> commands)
        {
            Commands = new CommandList(commands);
        }

        public CommandList Commands { get; set; }

        ICommandList IBucket<TContext>.Commands => Commands;

        public static Bucket<TContext> Play(string trackId, int milliseconds = 0)
        {
            return new Bucket<TContext>(new LoopCommand(SpotifyItemType.Track, trackId).AtMilliseconds(milliseconds));
        }

        public static Bucket<TContext> Transfer(string trackId, string mapping, IPattern<string> fromTrackId = null)
        {
            return new TransferCommand(SpotifyItemType.Track, trackId, mapping, fromTrackId).Then(new SetPlaybackOptionsCommand { RepeatMode = RepeatMode.Track });
        }

        public static Bucket<TContext> Transfer(string trackId, Switch<int, string> mapping, IPattern<string> fromTrackId = null)
        {
            return new TransferCommand(SpotifyItemType.Track, trackId, mapping, fromTrackId).Then(new SetPlaybackOptionsCommand { RepeatMode = RepeatMode.Track });
        }

        public static implicit operator Bucket<TContext>(CommandList cl) => new Bucket<TContext>(cl);

        public override TrackedResponse<TContext> GetBucket(TContext c) => TrackedResponse.Create(this);

        public override XElement ToXml()
        {
            var element = base.ToXml();

            foreach (var command in Commands)
            {
                element.Add(command?.ToXml());
            }

            return element;
        }

        public override string ToString() => Name ?? base.ToString() + $"({string.Join(", ", Commands)})";

        public override IReadOnlyRule<TContext> ToReadOnly() => new ReadOnlyBucket<TContext>(this);
    }
}
using Patterns;
using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Readonly;
using Spotify;
using Spotify.Commands;
using System.Xml.Linq;
using Utils;

namespace Rules.RuleTypes.Mutable
{
	public static class Bucket
	{ }

	public class Bucket<TContext, TOut> : Rule<TContext, TOut>, IBucket<TContext, TOut>
	{
		public Bucket(TOut output = default) => Output = output;

		public TOut Output { get; set; }

		TOut IBucket<TContext, TOut>.Output => Output;

		//TODO: move these spotify-specific methods to RuleExamples?
		public static Bucket<TContext, ICommandList> Play(string trackId, int? milliseconds = null)
		{
			LoopCommand loopCommand = new LoopCommand(SpotifyItemType.Track, trackId);

			if (milliseconds is int ms)
			{
				loopCommand = loopCommand.AtMilliseconds(ms);
			}

			return new Bucket<TContext, ICommandList>(new CommandList(loopCommand));
		}

		public static Bucket<TContext, ICommandList> Transfer(string trackId, string mapping, IPattern<string> fromTrackId = null)
		{
			return new TransferCommand(SpotifyItemType.Track, trackId, mapping, fromTrackId).Then(new SetPlaybackOptionsCommand { RepeatMode = RepeatMode.Track });
		}

		public static Bucket<TContext, ICommandList> Transfer(string trackId, Switch<int, string> mapping, IPattern<string> fromTrackId = null)
		{
			return new TransferCommand(SpotifyItemType.Track, trackId, mapping, fromTrackId).Then(new SetPlaybackOptionsCommand { RepeatMode = RepeatMode.Track });
		}

		public static implicit operator Bucket<TContext, TOut>(TOut cl) => new Bucket<TContext, TOut>(cl);

		public override TrackedResponse<TContext, TOut> GetBucket(TContext c) => TrackedResponse.Create(this);

		public override XElement ToXml()
		{
			var element = base.ToXml();

			if (Output is IXmlExportable exportable)
			{
				element.Add(exportable.ToXml());
			}
			else
			{
				element.Value = Output.ToString();
			}

			return element;
		}

		public override string ToString() => Name ?? base.ToString() + $"({Output})";

		public override IReadOnlyRule<TContext, TOut> ToReadOnly(RuleParser<TContext, TOut> ruleParser) => new ReadOnlyBucket<TContext, TOut>(this, ruleParser);
	}
}
﻿using MyRoR2;
using Patterns;
using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Readonly;
using Spotify;
using Spotify.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Rules.RuleTypes.Mutable
{
	using static TransferCommand;

	public class Bucket : Rule, IBucket
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

		ICommandList IBucket.Commands => Commands;

		public static Bucket Play(string trackId, int milliseconds = 0)
		{
			return new PlayCommand(SpotifyItemType.Track, trackId).AtMilliseconds(milliseconds).Then(new SetPlaybackOptionsCommand(RepeatMode.Track));
		}

		public static Bucket Transfer(string trackId, MathFunc mapping, IPattern<string> fromTrackId = null)
		{
			return new TransferCommand(SpotifyItemType.Track, trackId, mapping(ms), fromTrackId).Then(new SetPlaybackOptionsCommand(RepeatMode.Track));
		}

		public static Bucket Transfer(string trackId, Switch<int, MathFunc> mapping, IPattern<string> fromTrackId = null)
		{
			return new TransferCommand(SpotifyItemType.Track, trackId, mapping.Select(m => m(ms)), fromTrackId).Then(new SetPlaybackOptionsCommand(RepeatMode.Track));
		}

		public static implicit operator Bucket(CommandList cl) => new Bucket(cl);

		public override Bucket GetBucket(Context c) => this;

		public override XElement ToXml()
		{
			var element = base.ToXml();

			foreach (var command in Commands)
			{
				element.Add(command.ToXml());
			}

			return element;
		}

		public override string ToString() => Name ?? base.ToString() + $"({string.Join(", ", Commands)})";

		public override IReadOnlyRule ToReadOnly() => new ReadOnlyBucket(this);
	}
}
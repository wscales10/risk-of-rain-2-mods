using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Mutable;
using Spotify.Commands;

namespace Rules
{
	using static RuleCase<string, ICommandList>;

	public static class SimpleRule2
	{
		public static IReadOnlyRule<string, ICommandList> Instance { get; } = Switcher.StringToSpotify.Create
		(
			new Bucket<string, ICommandList>(new CommandList(new StopCommand())),
			C(Bucket<string, ICommandList>.Play("0VlxbRzdOyA56BviBK9vkc"), "Teleporter Charging"),
			C(Bucket<string, ICommandList>.Play("6Tfz4vOltixQpJFNaFvQ1H"), "Teleporter Idle")
		).ToReadOnly(RuleParser.StringToSpotify);
	}
}
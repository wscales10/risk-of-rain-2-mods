using MyRoR2;
using Patterns.Patterns.SmallPatterns.ValuePatterns;
using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Mutable;
using Spotify.Commands;
using System.Linq;
using static Patterns.Case;

namespace RuleExamples.RiskOfRain2
{
	using static RuleCase<string, ICommandList>;
	using static Bucket<string, ICommandList>;

	public static class MimicRule
	{
		private static readonly Rule<string, ICommandList> IdleRule = Switcher.StringToSpotify.Create(
					C(Play("0YPqWkd7ad07IJfaGDgBLE"), "Abandoned Aqueduct (Simulacrum)"),
					M("7qOPvD6VE4EsC6LU3VBCOR", "Idle", Scenes.DistantRoost),
					M("0ViueDwpuVR94Ny75ItU1P", "Idle", Scenes.AbandonedAqueduct, Scenes.AbyssalDepths),
					M("3NlFc4z24XefS6Orsz7zVG", "Idle", Scenes.TitanicPlains, Scenes.ScorchedAcres),
					M("3O4t4NB2N0RnPazn3Jf06I", "Idle", Scenes.RallypointDelta, Scenes.GildedCoast),
					M("2C3Nzw3gkEQaS6pXdeKG8Y", "Idle", Scenes.VoidFields),
					M("4oLjFQGMuljNXPVXDYlCjQ", "Idle", Scenes.WetlandAspect),
					M("6aBPlHW5g28spdHdvY19kb", "Idle", Scenes.BazaarBetweenTime),
					M("5KEoPtv1mCNpF9Ne75kozY", "Idle", Scenes.SkyMeadow),
					M("5tWWzN2tXSCOdinN6eOabW", "Idle", Scenes.SirensCall, Scenes.BulwarksAmbry),
					M("5cCjhYmgJwQwm5eEgttDxC", "Idle", Scenes.AMomentFractured, Scenes.AMomentWhole),
					M("2pl3Mzh2LeeUyzFacnHyZc", "Idle", Scenes.VoidLocus),
					M("75ENq958lhxLehA24dz72n", "Idle", Scenes.SiphonedForest),
					M("5MtbLsBAmW4VmXQvVfkvtc", "Idle", Scenes.AphelianSanctuary),
					M("6MiEIwSj9Z4UbxBdjxtBFB", "Idle", Scenes.SunderedGrove),
					M("0YPqWkd7ad07IJfaGDgBLE", "Idle", Scenes.SulfurPools)
			).Named("Other");

		private static readonly Rule<string, ICommandList> SpecialRule = Switcher.StringToSpotify.Create(
			C(new CommandList(new StopCommand()), "Pre-encounter"),
			C(Play("3KE5ossIfDAnBqNJFF8LfF", 6280), "Mithrix Phase 1"),
			C(Play("3KE5ossIfDAnBqNJFF8LfF", 85675), "Mithrix Phase 2"),
			C(Play("3KE5ossIfDAnBqNJFF8LfF", 148045), "Mithrix Phase 3"),
			C(Play("3KE5ossIfDAnBqNJFF8LfF", 259950), "Mithrix Phase 4"),
			C(new CommandList(new SetPlaybackOptionsCommand { RepeatMode = RepeatMode.Off }), "Mithrix Death Animation"),
			C(Play("7G349JbUH3PRdj5e7780Iz"), "Moon escape sequence"),
			C(Play("5fdiSSxvIsrCJ7sVkuhxnD"), "Voidling Fight"),
			C(Play("2pl3Mzh2LeeUyzFacnHyZc"), "The Planetarium (Idle)"));

		private static readonly Rule<string, ICommandList> BossRule = Switcher.StringToSpotify.Create(
			M("0j6CuhzD0XQlA5iTiRkx5P", "Teleporter", Scenes.TitanicPlains, Scenes.DistantRoost, Scenes.AbandonedAqueduct),
			M("4pcp0D8SyvuIans1NOtTht", "Teleporter", Scenes.WetlandAspect, Scenes.ScorchedAcres, Scenes.SirensCall),
			M("1XMfSk9yWjZdUNo1WuEIWF", "Teleporter", Scenes.RallypointDelta, Scenes.AbyssalDepths, Scenes.SunderedGrove),
			M("0F5ZMhyD9Msd7mRSFSHgY5", "Teleporter", Scenes.SkyMeadow),
			M("0G44J59yRCMqJp5k8JVYcz", "Teleporter", Scenes.SiphonedForest, Scenes.AphelianSanctuary),
			M("6uwL79qmtMJZA3Cxxd94c7", "Teleporter", Scenes.SulfurPools));

		private static readonly Rule<string, ICommandList> EnvironmentRule = Switcher.StringToSpotify.Create(
			ArrayRule.Create(BossRule, SpecialRule, IdleRule),
			C(SeekToCommand.AtSeconds(-23).Then(new SetPlaybackOptionsCommand { RepeatMode = RepeatMode.Off }), "Teleporter Charged"));

		private static readonly Rule<string, ICommandList> OtherRule = Switcher.StringToSpotify.Create(
			C(Play("6H6LnqmjEcW8gl6D7dIRKJ"), "Splash and Intro"),
			C(Main(), "Main Menu"),
			C(Play("3tEnANZ8EeFj3FDBEYXQxG"), "Outro"),
			C(Play("5cCjhYmgJwQwm5eEgttDxC", 42000), "Credits"),
			MultiCase.Create(_ => Dehydrated(), "Logbook", "Normal Character Select", "Eclipse Menus", "Prismatic Trial Menus"),
			C(Play("6umrQw3KWPFS6CQHN7J5BW"), "Simulacrum Menus"));

		public static IReadOnlyRule<string, ICommandList> Instance { get; } = ArrayRule.Create(
			EnvironmentRule, OtherRule).ToReadOnly(RuleParsers.StringToSpotify);

		private static Bucket<string, ICommandList> Dehydrated() => Transfer("3BBYYGOlrDKEVOQvXD4huf", Switch("ms", P<int, string>("ms - 8000", IntPattern.x > 68000)), StringPattern.Equals("0Q4NqUpiuOnQxYOGjnbuCh"));

		private static Bucket<string, ICommandList> Main() => Transfer("0Q4NqUpiuOnQxYOGjnbuCh", Switch("ms", P<int, string>("ms + 8000", IntPattern.x > 60000)), StringPattern.Equals("3BBYYGOlrDKEVOQvXD4huf"));

		private static MultiCase<string, string, ICommandList> M(string trackId, string descriptor, params DefinedScene[] sceneNames)
		{
			return MultiCase.Create(_ => Play(trackId), sceneNames.Select(sceneName => $"{sceneName} ({descriptor})").ToArray());
		}
	}
}
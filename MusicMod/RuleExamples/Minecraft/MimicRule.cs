using Patterns.Patterns.SmallPatterns.ValuePatterns;
using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Mutable;
using Spotify.Commands;
using System.Linq;

namespace RuleExamples.Minecraft
{
	using static RuleCase<string, ICommandList>;
	using static Bucket<string, ICommandList>;
	using static SpotifyTrackIds;

	public static class MimicRule
	{
		//TODO: split the logic for choosing a spotify item from the logic for choosing an action to take

		private static readonly Rule<string, ICommandList> CreativeOnly = PickOne(BiomeFest, BlindSpots, HauntMuskie, AriaMath, Dreiton, Taswell);

		private static readonly Rule<string, ICommandList> AllModes = PickOne(
			Minecraft,
			Clark,
			Sweden,
			SubwooferLullaby,
			LivingMice,
			Haggstrom,
			Danny,
			Key,
			Oxygène,
			DryHands,
			WetHands,
			MiceOnVenus);

		private static readonly Rule<string, ICommandList> AllBiomes = Switcher.StringToSpotify.Create(
			C(CreativeOnly, "Creative mode"),
			C(AllModes, "All modes"));

		private static readonly Rule<string, ICommandList> ForestOrJungle = PickOne(
			Aerie,
			ComfortingMemories,
			Firebugs,
			FloatingDream,
			Labyrinthine,
			LeftToBloom,
			OneMoreDay);

		private static readonly Rule<string, ICommandList> BiomeSpecific = Switcher.StringToSpotify.Create(
			C(ForestOrJungle, "Forest or Jungle"),
			C(Play(Ancestry), "Deep Dark"),
			C(PickOne(AnOrdinaryDay, InfiniteAmethyst, Wending), "Dripstone Caves"),
			C(Play(StandTall), "Frozen Peaks"),
			C(PickOne(ComfortingMemories, InfiniteAmethyst, Wending), "Grove"),
			C(PickOne(FloatingDream, StandTall, Wending), "Jagged Peaks"),
			C(PickOne(Aerie, AnOrdinaryDay, Firebugs, FloatingDream, Labyrinthine, LeftToBloom, OneMoreDay), "Lush Caves"),
			C(PickOne(LeftToBloom, OneMoreDay), "Meadow"),
			C(PickOne(ComfortingMemories, FloatingDream, LeftToBloom, OneMoreDay), "Old Growth Taiga"),
			C(PickOne(AnOrdinaryDay, OneMoreDay, StandTall), "Snowy Slopes"),
			C(PickOne(StandTall, Wending), "Stony Peaks"),
			C(PickOne(Aerie, Firebugs, Labyrinthine), "Swamp"));

		private static readonly Rule<string, ICommandList> Overworld = ArrayRule.Create(AllBiomes, BiomeSpecific);

		private static readonly Rule<string, ICommandList> Nether = Switcher.StringToSpotify.Create(
				C(PickOne(ConcreteHalls, DeadVoxel, Warmth, BalladOfTheCats), "The Nether"),
				C(Play(Chrysopoeia), "Crimson Forest"),
				C(Play(Rubedo), "Nether Wastes"),
				C(Play(SoBelow), "Basalt Deltas and Soul Sand Valley")
			);

		private static readonly Rule<string, ICommandList> NotUnderwater = Switcher.StringToSpotify.Create(
			ArrayRule.Create(Overworld, Nether),
			C(Play(TheEnd), "The End"),
			C(Play("4skiwoFHGqI7oF7JkpLr7N"), "Ender Dragon battle"));

		private static readonly Rule<string, ICommandList> Underwater = PickOne(Axolotl, DragonFish, Shuniji);

		private static readonly Rule<string, ICommandList> NoScreen = new IfRule<string, ICommandList>(StringPattern.Equals("Underwater"), Underwater, NotUnderwater);

		private static readonly RuleCase<string, string, ICommandList> MenuCase = C(PickOne(Mutation, MoogCity2, Beginning2, FloatingTrees, Aerie, Firebugs, Labyrinthine), "Menu");

		private static readonly RuleCase<string, string, ICommandList> EndPoemCase = C(Play(Alpha), "End Poem");

		public static IReadOnlyRule<string, ICommandList> Instance { get; } = Switcher.StringToSpotify.Create(
			NoScreen,
			MenuCase,
			EndPoemCase).ToReadOnly(RuleParsers.StringToSpotify);

		private static ArrayRule<string, ICommandList> PickOne(params string[] trackIds)
		{
			return ArrayRule.Create(trackIds.Select(id => Play(id))).MakeRandom();
		}
	}
}
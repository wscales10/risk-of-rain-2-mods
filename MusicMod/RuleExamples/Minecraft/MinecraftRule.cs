using Minecraft;
using Patterns.Patterns;
using Patterns.Patterns.CollectionPatterns;
using Patterns.Patterns.SmallPatterns.ValuePatterns;
using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Mutable;
using System.Collections.Generic;
using Utils;

namespace RuleExamples.Minecraft
{
	using static RuleCase<MinecraftContext, string>;
	using Query = Query<MinecraftContext>;

	public static class MinecraftRule
	{
		private static readonly Rule<MinecraftContext, string> Menu = "Menu";

		private static readonly Rule<MinecraftContext, string> EndPoem = "End Poem";

		private static readonly Rule<MinecraftContext, string> Creative = ArrayRule.Create<MinecraftContext, string>(
			"Creative mode",
			"All modes");

		private static readonly Rule<MinecraftContext, string> AllBiomes = new IfRule<MinecraftContext, string>(
			Query.Create<GameMode>(nameof(MinecraftContext.GameMode), EnumRangePattern.Equals(GameMode.Creative)),
			Creative,
			"All modes");

		private static readonly Rule<MinecraftContext, string> BiomeSpecific = Switcher.MinecraftToString.Create(
			nameof(MinecraftContext.Biome),
			AllBiomes,
			b => new BiomePattern().DefineWith(b.ToString()),
			C("Forest or Jungle", Biomes.Forest, Biomes.Jungle),
			C("Deep Dark", Biomes.DeepDark),
			C("Dripstone Caves", Biomes.DripstoneCaves),
			C("Frozen Peaks", Biomes.Mountains.FrozenPeaks),
			C("Grove", Biomes.Mountains.Grove),
			C("Jagged Peaks", Biomes.Mountains.JaggedPeaks),
			C("Lush Caves", Biomes.LushCaves),
			C("Meadow", Biomes.Mountains.Meadow),
			C("Old Growth Taiga", Biomes.OldGrowthTaiga),
			C("Snowy Slopes", Biomes.Mountains.SnowySlopes),
			C("Stony Peaks", Biomes.Mountains.StonyPeaks),
			C("Swamp", Biomes.Swamp));

		private static readonly Rule<MinecraftContext, string> Overworld = ArrayRule.Create(
		AllBiomes,
		BiomeSpecific)
		.MakeRandom();

		private static readonly Rule<MinecraftContext, string> Nether = ArrayRule.Create(
			"The Nether",
			Switcher.MinecraftToString.Create(
				nameof(MinecraftContext.Biome),
				"The Nether",
				C("Crimson Forest", Biomes.CrimsonForest),
				C("Nether Wastes", Biomes.NetherWastes),
				C("Basalt Deltas and Soul Sand Valley", Biomes.BasaltDeltas, Biomes.SoulSandValley)
			)
		)
		.MakeRandom();

		private static readonly Rule<MinecraftContext, string> End = new IfRule<MinecraftContext, string>(
			Query.Create(nameof(MinecraftContext.Bosses), new AnyPattern<Mob>(new MobPattern().DefineWith(Mobs.EnderDragon.Id))),
			"Ender Dragon battle",
			"The End");

		private static readonly Rule<MinecraftContext, string> NotUnderwater = Switcher.MinecraftToString.Create(
			nameof(MinecraftContext.Dimension),
			C(Overworld, Dimension.Overworld).Named(nameof(Overworld)),
			C(Nether, Dimension.Nether).Named(nameof(Nether)),
			C(End, Dimension.End).Named(nameof(End)));

		private static readonly Rule<MinecraftContext, string> Underwater = "Underwater";

		private static readonly Rule<MinecraftContext, string> NoScreen = new IfRule<MinecraftContext, string>(Query.Create<bool>(nameof(MinecraftContext.IsUnderwater), BoolPattern.True), Underwater, NotUnderwater);

		public static IReadOnlyRule<MinecraftContext, string> Instance { get; } = Switcher.MinecraftToString.Create(
			nameof(MinecraftContext.Screen),
			NoScreen,
			C<IScreen>(Menu, Screens.MainMenu, Screens.WorldSelect).Named(nameof(Menu)),
			C<IScreen>(EndPoem, Screens.EndPoem).Named(nameof(EndPoem))).ToReadOnly(RuleParsers.MinecraftToString);
	}
}
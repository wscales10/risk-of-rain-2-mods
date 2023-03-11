namespace Minecraft
{
	public static class Biomes
	{
		public static Biome CrimsonForest { get; } = B("crimson_forest");

		public static Biome NetherWastes { get; } = B("nether_wastes");

		public static Biome BasaltDeltas { get; } = B("basalt_deltas");

		public static Biome SoulSandValley { get; } = B("soul_sand_valley");

		public static Biome DeepDark { get; } = B("deep_dark");

		public static Biome DripstoneCaves { get; } = B("dripstone_caves");

		public static Biome Forest { get; } = B("forest");

		public static Biome FlowerForest { get; } = Forest.Variant("flower_forest");

		public static Biome Jungle { get; } = B("jungle");

		public static Biome SparseJungle { get; } = Jungle.Variant("sparse_jungle");

		public static Biome BambooJungle { get; } = Jungle.Variant("bamboo_jungle");

		public static Biome LushCaves { get; } = B("lush_caves");

		public static Biome OldGrowthTaiga { get; } = new("_", "old_growth_taiga");

		public static Biome OldGrowthPineTaiga { get; } = B("old_growth_pine_taiga");

		public static Biome OldGrowthSpruceTaiga { get; } = B("old_growth_spruce_taiga");

		public static Biome Swamp { get; } = B("swamp");

		public static Biome MangroveSwamp { get; } = Swamp.Variant("mangrove_swamp");

		private static Biome B(string path) => new("minecraft", path);

		public static class Mountains
		{
			public static Biome Meadow { get; } = B("meadow");

			public static Biome Grove { get; } = B("grove");

			public static Biome SnowySlopes { get; } = B("snowy_slopes");

			public static Biome JaggedPeaks { get; } = B("jagged_peaks");

			public static Biome FrozenPeaks { get; } = B("frozen_peaks");

			public static Biome StonyPeaks { get; } = B("stony_peaks");
		}
	}
}
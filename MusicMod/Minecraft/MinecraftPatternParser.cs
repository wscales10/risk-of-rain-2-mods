using Patterns;
using Patterns.Patterns;
using Patterns.Patterns.SmallPatterns.ValuePatterns;
using Patterns.TypeDefs;

namespace Minecraft
{
	public class MinecraftPatternParser : PatternParser
	{
		protected MinecraftPatternParser()
		{
		}

		public static new MinecraftPatternParser Instance { get; } = new MinecraftPatternParser();

		protected override bool TryGetTypeDefGetter(string typeKey, out ITypeDefGetter typeDefGetter)
		{
			switch (typeKey)
			{
				case nameof(Biome):
					typeDefGetter = BiomePattern.TypeDef;
					return true;

				case nameof(Dimension):
					typeDefGetter = DimensionPattern.TypeDef;
					return true;

				case nameof(IScreen):
					typeDefGetter = ScreenPattern.TypeDef;
					return true;

				case nameof(Mob):
					typeDefGetter = MobPattern.TypeDef;
					return true;

				default:
					return base.TryGetTypeDefGetter(typeKey, out typeDefGetter);
			}
		}
	}
}
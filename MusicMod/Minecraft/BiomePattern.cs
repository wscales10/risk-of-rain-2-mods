using Patterns.Patterns.SmallPatterns;
using Patterns.TypeDefs;
using System.Text.RegularExpressions;

namespace Minecraft
{
	public class BiomePattern : ClassValuePattern<Biome>
	{
		private static readonly Regex regex = new(@"(?<base>[^/]*) \/ (?<biome>[^/]*)");

		private string? baseString;

		private string? biomeString;

		internal static TypeDef TypeDef { get; } = TypeDef.Create<Biome, BiomePattern>((s) => (BiomePattern)new BiomePattern().DefineWith(s), s => Equals(s));

		public static BiomePattern Equals(Biome b)
		{
			return (BiomePattern)new BiomePattern().DefineWith(b.ToString());
		}

		protected override bool defineWith(string stringDefinition)
		{
			var match = regex.Match(stringDefinition);

			if (!match.Success)
			{
				return false;
			}

			var baseString = match.Groups["base"].Value;
			var biomeString = match.Groups["biome"].Value;

			if (baseString == "*" || baseString is null)
			{
				return false;
			}

			if (biomeString == "*" && Biome.TryGet(b => b.Base.AsString() == baseString, out _)
				|| biomeString is not null && Biome.TryGet(b => b.ToString() == stringDefinition, out _))
			{
				this.baseString = baseString;
				this.biomeString = biomeString;
				return true;
			}

			return false;
		}

		protected override bool isMatch(Biome value)
		{
			if (value.Base.AsString() == baseString)
			{
				return biomeString == "*" || value.AsString() == biomeString;
			}

			return false;
		}
	}
}
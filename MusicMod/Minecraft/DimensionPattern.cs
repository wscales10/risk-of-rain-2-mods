using Patterns.Patterns.SmallPatterns;
using Patterns.TypeDefs;
using System.Text.RegularExpressions;

namespace Minecraft
{
	public class DimensionPattern : ClassValuePattern<Dimension>
	{
		private Dimension? dimension;

		internal static TypeDef TypeDef { get; } = TypeDef.Create<Dimension, DimensionPattern>((s) => (DimensionPattern)new DimensionPattern().DefineWith(s), s => Equals(s));

		public static DimensionPattern Equals(Dimension b)
		{
			return (DimensionPattern)new DimensionPattern().DefineWith(b.ToString());
		}

		protected override bool defineWith(string stringDefinition)
		{
			var dim = Dimension.Get(d => d.ToString() == stringDefinition);

			if (dim is null)
			{
				return false;
			}

			dimension = dim;
			return true;
		}

		protected override bool isMatch(Dimension value)
		{
			return value == dimension;
		}
	}
}
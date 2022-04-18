using Patterns.Patterns.SmallPatterns.ValuePatterns;
using Patterns.TypeDefs;
using System;
using Utils;

namespace Patterns.Patterns.SmallPatterns
{
	public static class NullablePattern
	{
		private static IPattern Equalizer(Type t, object x, PatternParser patternParser)
		{
			if (x is null)
			{
				return typeof(NullableNullPattern<>).MakeGenericType(t.GenericTypeArguments[0]).GetStaticPropertyValue<IPattern>("IsNull");
			}
			else
			{
				return (IPattern)typeof(PatternParser).GetMethod("GetEqualizer").MakeGenericMethod(x.GetType()).Invoke(patternParser, new[] { x });
			}
		}

		internal static ParserSpecificTypeDefGetter GenericTypeDef { get; }
			= new ParserSpecificTypeDefGetter(
				patternParser => new GenericTypeDef(
					gta => new TypeDef(
						(_, __, ___) => throw new InvalidOperationException(),
						(t, x) => Equalizer(t, x, patternParser),
						typeof(Nullable<>).MakeGenericType(gta),
						null)));
	}
}

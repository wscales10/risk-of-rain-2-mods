using Patterns.TypeDefs;
using System;
using Utils.Reflection.Properties;

namespace Patterns.Patterns.SmallPatterns
{
	public static class NullablePattern
	{
		internal static ParserSpecificTypeDefGetter GenericTypeDef { get; }
			= new ParserSpecificTypeDefGetter(
				patternParser => new GenericArgTypeDef(
					gta => new TypeDef(
						(_, __) => throw new InvalidOperationException(),
						(t, x) => Equalizer(t, x, patternParser),
						typeof(Nullable<>).MakeGenericType(gta),
						null)));

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
	}
}
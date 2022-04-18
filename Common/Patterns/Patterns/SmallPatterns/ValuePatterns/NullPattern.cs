using Patterns.TypeDefs;
using System;
using Utils;

namespace Patterns.Patterns.SmallPatterns
{
	public sealed class NullPattern : ValuePattern<object>
	{
		private bool shouldBeNull;

		internal NullPattern()
		{
		}

		public static NullPattern IsNull => (NullPattern)new NullPattern().DefineWith("null");

		public static NullPattern IsNotNull => (NullPattern)new NullPattern().DefineWith("not null");

		internal static TypeDef TypeDef { get; } = TypeDef.Create<object, NullPattern>((s, _, __) => (NullPattern)new NullPattern().DefineWith(s), x => Equals(x));

		internal static ParserSpecificTypeDefGetter NullableTypeDef { get; }
			= new ParserSpecificTypeDefGetter(
				patternParser => new GenericTypeDef(
					gta => new TypeDef(
						(s, t, a) => NullableNullPattern.Definer(s, t, a, patternParser),
						NullableNullPattern.Equalizer,
						typeof(Nullable<>).MakeGenericType(gta),
						(_) => typeof(NullableNullPattern<>).MakeGenericType(gta)
						)
					)
				);

		internal static ParserSpecificTypeDefGetter ClassTypeDef { get; }
			= new ParserSpecificTypeDefGetter(
				patternParser => new TypeDef(
					(s, t, a) => ClassNullPattern.Definer(s, t, a, patternParser),
					ClassNullPattern.Equalizer,
					typeof(object),
					(t) => typeof(ClassNullPattern<>).MakeGenericType(t)
					)
				);

		private new static NullPattern Equals(object value)
		{
			return value is null ? IsNull : throw new InvalidOperationException();
		}

		protected override bool isMatch(object value)
		{
			return shouldBeNull == (value is null);
		}

		protected override bool defineWith(string stringDefinition)
		{
			switch (stringDefinition)
			{
				case "null":
					shouldBeNull = true;
					return true;

				case "not null":
					shouldBeNull = false;
					return true;

				default:
					return false;
			}
		}
	}
}
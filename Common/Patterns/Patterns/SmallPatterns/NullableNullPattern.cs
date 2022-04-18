using System;
using Utils;

namespace Patterns.Patterns.SmallPatterns
{
	internal static class NullableNullPattern
	{
		public static PatternBase Equalizer(object x)
		{
			var constructedGenericType = typeof(NullableNullPattern<>).MakeGenericType(x.GetType());
			return (PatternBase)constructedGenericType.GetMethod("Equals").InvokeStatic(x);
		}

		public static IPattern Definer(string s, string t, string a, PatternParser patternParser)
		{
			if (a == "Enum`1")
			{
				return (ClassNullPattern<Enum>)new NullPattern().DefineWith(s);
			}
			else
			{
				return (IPattern)typeof(NullableNullPattern<>).MakeGenericType(patternParser.GetType(a)).Construct(new NullPattern().DefineWith(s));
			}
		}
	}

	public sealed class NullableNullPattern<T> : SmallPattern<T?>
		where T : struct
	{
		private readonly NullPattern basePattern;

		internal NullableNullPattern(NullPattern nullPattern) => basePattern = nullPattern;

		public static NullableNullPattern<T> IsNull { get; } = new NullableNullPattern<T>(NullPattern.IsNull);

		public static NullableNullPattern<T> IsNotNull { get; } = new NullableNullPattern<T>(NullPattern.IsNotNull);

		public override string Definition => basePattern.Definition;

		public static NullableNullPattern<T> Equals(T? value) => value is null ? IsNull : throw new NotImplementedException();

		public override bool IsMatch(T? value) => basePattern.IsMatch(value);

		public override string TypeKey => "nullable-null";

		public static explicit operator NullableNullPattern<T>(NullPattern np) => new NullableNullPattern<T>(np);

		public override IPattern Correct()
		{
			if (IsNullableEnumPattern)
			{
				return new ClassNullPattern<Enum>(basePattern);
			}
			else
			{
				return base.Correct();
			}
		}
	}
}

using Patterns.TypeDefs;
using System;
using System.Linq;
using Utils.Reflection;

namespace Patterns.Patterns.SmallPatterns
{
	public sealed class NullableNullPattern<T> : SmallPattern<T?>
		where T : struct
	{
		private readonly NullPattern basePattern;

		internal NullableNullPattern(NullPattern nullPattern) => basePattern = nullPattern;

		public static NullableNullPattern<T> IsNull { get; } = new NullableNullPattern<T>(NullPattern.IsNull);

		public static NullableNullPattern<T> IsNotNull { get; } = new NullableNullPattern<T>(NullPattern.IsNotNull);

		public override string Definition => basePattern.Definition;

		public override string TypeKey => "nullable-null";

		public static NullableNullPattern<T> Equals(T? value) => value is null ? IsNull : throw new NotImplementedException();

		public static explicit operator NullableNullPattern<T>(NullPattern np) => new NullableNullPattern<T>(np);

		public override bool IsMatch(T? value) => basePattern.IsMatch(value);

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

	internal static class NullableNullPattern
	{
		public static PatternBase Equalizer(object x)
		{
			var constructedGenericType = typeof(NullableNullPattern<>).MakeGenericType(x.GetType());
			return (PatternBase)constructedGenericType.GetMethod("Equals").InvokeStatic(x);
		}

		public static IPattern Definer(string s, TypeRef typeRef, PatternParser patternParser)
		{
			string a = typeRef.GenericTypeKeys.Single();
			if (typeRef.GenericTypeKeys.Single() == "Enum`1")
			{
				return (ClassNullPattern<Enum>)new NullPattern().DefineWith(s);
			}
			else
			{
				return (IPattern)typeof(NullableNullPattern<>).MakeGenericType(patternParser.GetType(a)).Construct(new NullPattern().DefineWith(s));
			}
		}
	}
}
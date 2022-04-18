using System;
using Utils;

namespace Patterns.Patterns.SmallPatterns
{
	public static class ClassNullPattern
	{
		public static PatternBase Equalizer(object x)
		{
			var constructedGenericType = typeof(ClassNullPattern<>).MakeGenericType(x.GetType());
			return (PatternBase)constructedGenericType.GetMethod("Equals").InvokeStatic(x);
		}

		public static IPattern Definer(string s, string t, string a, PatternParser patternParser)
		{
			var type = typeof(ClassNullPattern<>).MakeGenericType(patternParser.GetType(t, a));
			return (IPattern)type.Construct(new NullPattern().DefineWith(s));
		}
	}

	public sealed class ClassNullPattern<T> : SmallPattern<T>
		where T : class
	{
		private readonly NullPattern basePattern;

		internal ClassNullPattern(NullPattern nullPattern) => basePattern = nullPattern;

		public static ClassNullPattern<T> IsNull { get; } = new ClassNullPattern<T>(NullPattern.IsNull);

		public static ClassNullPattern<T> IsNotNull { get; } = new ClassNullPattern<T>(NullPattern.IsNotNull);

		public override string Definition => basePattern.Definition;

		public static ClassNullPattern<T> Equals(T value) => value is null ? IsNull : throw new NotImplementedException();

		public override bool IsMatch(T value) => basePattern.IsMatch(value);

		public override string TypeKey => "class-null";

		public static explicit operator ClassNullPattern<T>(NullPattern np) => new ClassNullPattern<T>(np);
	}
}

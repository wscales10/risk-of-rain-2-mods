using Patterns.TypeDefs;
using System;
using Utils.Reflection;

namespace Patterns.Patterns.SmallPatterns
{
	public static class ClassNullPattern
	{
		public static PatternBase Equalizer(object x)
		{
			var constructedGenericType = typeof(ClassNullPattern<>).MakeGenericType(x.GetType());
			return (PatternBase)constructedGenericType.GetMethod("Equals").InvokeStatic(x);
		}

		public static IPattern Definer(string s, TypeRef typeRef, PatternParser patternParser)
		{
			var type = typeof(ClassNullPattern<>).MakeGenericType(patternParser.GetType(typeRef));
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

		public override string TypeKey => "class-null";

		public static ClassNullPattern<T> Equals(T value) => value is null ? IsNull : throw new NotImplementedException();

		public static explicit operator ClassNullPattern<T>(NullPattern np) => new ClassNullPattern<T>(np);

		public override bool IsMatch(T value) => basePattern.IsMatch(value);
	}
}
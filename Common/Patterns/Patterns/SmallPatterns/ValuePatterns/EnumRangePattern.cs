using System;
using System.Text.RegularExpressions;
using Patterns.TypeDefs;
using Utils;
using Utils.Reflection;

namespace Patterns.Patterns.SmallPatterns.ValuePatterns
{
	public class EnumRangePattern : ClassValuePattern<Enum>
	{
		private readonly IntPattern basePattern = new IntPattern();

		private string minString;

		private string maxString;

		internal static TypeDef TypeDef { get; } = TypeDef.Create<Enum, EnumRangePattern>((s) => (EnumRangePattern)new EnumRangePattern().DefineWith(s), e => Equals(e));

		internal static ParserSpecificTypeDefGetter GenericTypeDef { get; } = new ParserSpecificTypeDefGetter(
				patternParser => new BestTypeDefGetter(typeRef =>
					typeRef.FullType is null ? TypeDef : new TypeDef(
					(s) => GenericDefiner(s, typeRef, patternParser),
					Equalizer,
					typeRef.DenullabledType,
					(t) => typeof(EnumRangePattern<>).MakeGenericType(t))
					)
				);

		internal static Regex Regex { get; } = new Regex(@"^(?<item1>(?<string1>.*?)\((?<num1>-?\d+)\))?(?<dots>\.\.)?(?<item2>(?<string2>.*)\((?<num2>-?\d+)\))?$");

		private (string, int)? Min => Ensure(minString, basePattern.Min);

		private (string, int)? Max => Ensure(maxString, basePattern.Max);

		public static void Validate(Type type, int index, string expectedName)
		{
			if (!Inherits(type, typeof(Enum)))
			{
				throw new ArgumentOutOfRangeException(nameof(type), type, "Not an enum");
			}

			string actualName = Enum.GetName(type, index);

			if (actualName != expectedName)
			{
				throw new Exception($"Expected {type}[{index}] to be {expectedName} but found {actualName}.");
			}
		}

		public static EnumRangePattern<T> Equals<T>(T value)
				where T : struct, Enum
		{
			return (EnumRangePattern<T>)new EnumRangePattern<T>().DefineWith($"{value}({EnumRangePattern<T>.GetIndex(value)})");
		}

		public static EnumRangePattern Equals(Enum value)
		{
			return (EnumRangePattern)new EnumRangePattern().DefineWith($"{value}({GetIndex(value)})");
		}

		public static int GetIndex(Enum value)
		{
			return (int)Convert.ChangeType(value, value.GetTypeCode());
		}

		public Enum GetMinValue(Type enumType) => minString is null ? null : (Enum)Enum.Parse(enumType, minString);

		public Enum GetMaxValue(Type enumType) => maxString is null ? null : (Enum)Enum.Parse(enumType, maxString);

		internal static (string, int)? Ensure(string s, int? n)
		{
			if (s is null)
			{
				if (n is null)
				{
					return null;
				}
			}
			else if (n is int i)
			{
				return (s, i);
			}

			throw new NullReferenceException();
		}

		protected override bool defineWith(string stringDefinition)
		{
			var match = Regex.Match(stringDefinition);

			if (!match.Success)
			{
				return false;
			}

			var dotGroup = match.Groups["dots"];

			var string1Group = match.Groups["string1"];
			var num1Group = match.Groups["num1"];
			var string2Group = match.Groups["string2"];
			var num2Group = match.Groups["num2"];

			basePattern.DefineWith($"{ num1Group.Value }{ dotGroup.Value }{ num2Group.Value }");
			if (basePattern.Definition is null)
			{
				return false;
			}
			else
			{
				minString = basePattern.Min is null ? null : string1Group.Value;
				maxString = basePattern.Max is null ? null : string2Group.Value;

				if (maxString?.Length == 0)
				{
					maxString = minString;
				}

				return true;
			}
		}

		protected override bool isMatch(Enum value)
		{
			if (Min.HasValue)
			{
				Validate(value.GetType(), Min.Value.Item2, Min.Value.Item1);
			}

			if (Max.HasValue)
			{
				Validate(value.GetType(), Max.Value.Item2, Max.Value.Item1);
			}

			var converted = Convert.ChangeType(value, value.GetTypeCode());
			return (Min is null || (int)converted >= Min.Value.Item2) && (Max is null || (int)converted <= Max.Value.Item2);
		}

		private static IPattern GenericDefiner(string s, TypeRef typeRef, PatternParser patternParser)
		{
			var pattern = typeof(EnumRangePattern<>).MakeGenericType(patternParser.GetType(typeRef)).Construct();
			return pattern.InvokeMethod<IPattern>(nameof(DefineWith), s);
		}

		private static PatternBase Equalizer(object x)
		{
			return (PatternBase)typeof(EnumRangePattern)
				.GetMethod("Equals", mi => mi.IsGenericMethod)
				.MakeGenericMethod(x.GetType())
				.InvokeStatic(x);
		}
	}

	public class EnumRangePattern<T> : StructValuePattern<T>
		where T : struct, Enum
	{
		private readonly IntPattern basePattern = new IntPattern();

		private string minString;

		private string maxString;

		public T? MinValue => minString is null ? (T?)null : minString.AsEnum<T>();

		public T? MaxValue => maxString is null ? (T?)null : maxString.AsEnum<T>();

		private (string, int)? Min => EnumRangePattern.Ensure(minString, basePattern.Min);

		private (string, int)? Max => EnumRangePattern.Ensure(maxString, basePattern.Max);

		public static EnumRangePattern<T> Create(T? min, T? max)
		{
			string part1, part2, part3;

			part1 = min is null ? null : $"{min}({GetIndex(min.Value)})";
			part2 = !(min is null) && Equals(min, max) ? null : "..";
			part3 = Equals(min, max) ? null : $"{max}({GetIndex(max.Value)})";

			return (EnumRangePattern<T>)new EnumRangePattern<T>().DefineWith($"{part1}{part2}{part3}");
		}

		public static void Validate(int index, string expectedName)
		{
			EnumRangePattern.Validate(typeof(T), index, expectedName);
		}

		public static int GetIndex(T value)
		{
			return (int)Convert.ChangeType(value, value.GetTypeCode());
		}

		public override IPattern Correct() => new EnumRangePattern().DefineWith(Definition);

		protected override bool defineWith(string stringDefinition)
		{
			var match = EnumRangePattern.Regex.Match(stringDefinition);

			if (!match.Success)
			{
				return false;
			}

			var dotGroup = match.Groups["dots"];

			var string1Group = match.Groups["string1"];
			var num1Group = match.Groups["num1"];

			if (string1Group.Success)
			{
				Validate(int.Parse(num1Group.Value), string1Group.Value);
			}

			var string2Group = match.Groups["string2"];
			var num2Group = match.Groups["num2"];

			if (string2Group.Success)
			{
				Validate(int.Parse(num2Group.Value), string2Group.Value);
			}

			basePattern.DefineWith($"{ num1Group.Value }{ dotGroup.Value }{ num2Group.Value }");

			if (basePattern.Definition is null)
			{
				return false;
			}
			else
			{
				minString = basePattern.Min is null ? null : string1Group.Value;
				maxString = basePattern.Max is null ? null : string2Group.Value;

				if (maxString?.Length == 0)
				{
					maxString = minString;
				}

				return true;
			}
		}

		protected override bool isMatch(T value)
		{
			var index = GetIndex(value);
			return (Min is null || index >= Min.Value.Item2) && (Max is null || index <= Max.Value.Item2);
		}
	}
}
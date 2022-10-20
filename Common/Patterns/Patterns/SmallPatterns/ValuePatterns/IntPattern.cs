using Patterns.TypeDefs;
using System;
using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;

namespace Patterns.Patterns.SmallPatterns.ValuePatterns
{
	[SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
	public class IntPattern : StructValuePattern<int>, IOrSimplifiable<int>, IAndSimplifiable<int>
	{
		public static readonly IntX x;

		private static readonly Regex regex = new Regex(@"^(?<num1>-?\d+)?(?<dots>\.\.)?(?<num2>-?\d+)?$");

		public int? Max { get; private set; }

		public int? Min { get; private set; }

		internal static TypeDef TypeDef { get; } = TypeDef.Create<int, IntPattern>((s) => (IntPattern)new IntPattern().DefineWith(s), i => (IntPattern)(x == i));

		public static IPattern<int> Create(int? min, int? max)
		{
			if (min is null && max is null)
			{
				return ConstantPattern<int>.True;
			}
			else
			{
				return new IntPattern().DefineWith($"{min}..{max}");
			}
		}

		public static Pattern<int> operator <(IntPattern ip, int? i) => (Pattern<int>)(ip & x < i).Simplify();

		public static Pattern<int> operator <=(IntPattern ip, int? i) => (Pattern<int>)(ip & x <= i).Simplify();

		public static Pattern<int> operator >(IntPattern ip, int? i) => throw new InvalidOperationException();

		public static Pattern<int> operator >=(IntPattern ip, int? i) => throw new InvalidOperationException();

		public IPattern<int> SimplifyAnd(IPattern<int> other)
		{
			if (!(other is IntPattern ip))
			{
				return null;
			}

			if (Definition is null || ip.Definition is null)
			{
				return null;
			}

			if (!(Max is null) && !(ip.Min is null) && ip.Min - Max > 0)
			{
				return ConstantPattern<int>.False;
			}

			if (!(ip.Max is null) && !(Min is null) && Min - ip.Max > 0)
			{
				return ConstantPattern<int>.False;
			}

			int? min = Min is null ? ip.Min : ip.Min is null ? Min : Math.Max(Min.Value, ip.Min.Value);
			int? max = Max is null ? ip.Max : ip.Max is null ? Max : Math.Min(Max.Value, ip.Max.Value);

			return Create(min, max);
		}

		public IPattern<int> SimplifyOr(IPattern<int> other)
		{
			if (!(other is IntPattern ip))
			{
				return null;
			}

			if (Definition is null || ip.Definition is null)
			{
				return null;
			}

			if (!(Max is null) && !(ip.Min is null) && ip.Min - Max > 1)
			{
				return null;
			}

			if (!(ip.Max is null) && !(Min is null) && Min - ip.Max > 1)
			{
				return null;
			}

			int? min = (Min is null || ip.Min is null) ? null : (int?)Math.Min(Min.Value, ip.Min.Value);
			int? max = (Max is null || ip.Max is null) ? null : (int?)Math.Max(Max.Value, ip.Max.Value);

			return Create(min, max);
		}

		protected override bool defineWith(string stringDefinition)
		{
			var match = regex.Match(stringDefinition);

			if (!match.Success)
			{
				return false;
			}

			var minGroup = match.Groups["num1"];
			var dotGroup = match.Groups["dots"];
			var maxGroup = match.Groups["num2"];

			Min = null;
			Max = null;

			if (minGroup.Success)
			{
				var num1 = int.Parse(minGroup.Value);
				if (maxGroup.Success)
				{
					var num2 = int.Parse(maxGroup.Value);
					if (dotGroup.Success && num1 <= num2)
					{
						Min = num1;
						Max = num2;
						return true;
					}
				}
				else
				{
					Min = num1;

					if (!dotGroup.Success)
					{
						Max = num1;
					}

					return true;
				}
			}
			else if (dotGroup.Success)
			{
				if (maxGroup.Success)
				{
					Max = int.Parse(maxGroup.Value);
				}

				return true;
			}

			return false;
		}

		protected override bool isMatch(int value) => (Min is null || value >= Min) && (Max is null || value <= Max);
	}
}
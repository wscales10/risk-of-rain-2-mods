using Patterns.TypeDefs;

namespace Patterns.Patterns.SmallPatterns.ValuePatterns
{
	public class BoolPattern : StructValuePattern<bool>, IAndSimplifiable<bool>, IOrSimplifiable<bool>, INotSimplifiable<bool>
	{
		private bool boolean;

		public static BoolPattern False => (BoolPattern)new BoolPattern().DefineWith("False");

		public static BoolPattern True => (BoolPattern)new BoolPattern().DefineWith("True");

		internal static TypeDef TypeDef { get; } = TypeDef.Create<bool, BoolPattern>((s) => (BoolPattern)new BoolPattern().DefineWith(s), b => Equals(b));

		public static BoolPattern Equals(bool b) => b ? True : False;

		public static BoolPattern operator !(BoolPattern bp) => (BoolPattern)new BoolPattern().DefineWith((!bp.boolean).ToString());

		public IPattern<bool> SimplifyAnd(IPattern<bool> other)
		{
			if (!(other is BoolPattern bp))
			{
				return null;
			}

			if (Definition is null || bp.Definition is null)
			{
				return null;
			}

			if (boolean == bp.boolean)
			{
				return this;
			}
			else
			{
				return ConstantPattern<bool>.False;
			}
		}

		public IPattern<bool> SimplifyNot() => Definition is null ? null : !this;

		public IPattern<bool> SimplifyOr(IPattern<bool> other)
		{
			if (!(other is BoolPattern bp))
			{
				return null;
			}

			if (Definition is null || bp.Definition is null)
			{
				return null;
			}

			if (boolean == bp.boolean)
			{
				return this;
			}
			else
			{
				return ConstantPattern<bool>.True;
			}
		}

		protected override bool defineWith(string stringDefinition) => bool.TryParse(stringDefinition, out boolean);

		protected override bool isMatch(bool value) => value == boolean;
	}
}
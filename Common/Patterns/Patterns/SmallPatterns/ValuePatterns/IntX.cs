using System.Diagnostics.CodeAnalysis;

namespace Patterns.Patterns.SmallPatterns.ValuePatterns
{
	[SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
	public class IntX
	{
		public static Pattern<int> operator !=(IntX x, int? i) => !(x == i);

		public static IntPattern operator <(IntX x, int? i) => (IntPattern)new IntPattern().DefineWith($"..{(i is null ? i : i - 1)}");

		public static IntPattern operator <(int? i, IntX x) => x > i;

		public static IntPattern operator <=(IntX x, int? i) => (IntPattern)new IntPattern().DefineWith($"..{i}");

		public static IntPattern operator <=(int? i, IntX x) => x >= i;

		public static Pattern<int> operator ==(IntX x, int? i) => new IntPattern().DefineWith(i?.ToString() ?? "..");

		public static IntPattern operator >(IntX x, int? i) => (IntPattern)new IntPattern().DefineWith($"{(i is null ? i : i + 1)}..");

		public static IntPattern operator >(int? i, IntX x) => x < i;

		public static IntPattern operator >=(IntX x, int? i) => (IntPattern)new IntPattern().DefineWith($"{i}..");

		public static IntPattern operator >=(int? i, IntX x) => x <= i;
	}
}
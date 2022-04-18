using System.Collections.Generic;
using System.Linq;

namespace Patterns
{
	public static class Case
	{
		private static Case<TValue, TOut> C<TValue, TOut>(TOut output, params TValue[] arr) => new Case<TValue, TOut>(output, arr);

		public static Case<IPattern<T>, TOut> P<T, TOut>(TOut output, params IPattern<T>[] arr) => C(output, arr);

		public static Switch<T, TOut> Switch<T, TOut>(TOut defaultOutput, params Case<IPattern<T>, TOut>[] cases) => new Switch<T, TOut>(defaultOutput, cases);
	}

	public class Case<TValue, TOut>
	{
		public Case(TOut output, params TValue[] arr) : this(output, (IEnumerable<TValue>)arr)
		{
		}

		public Case(TOut output, IEnumerable<TValue> arr)
		{
			Output = output;
			Arr = arr.ToList();
		}

		public List<TValue> Arr { get; }

		public TOut Output { get; set; }

		public static implicit operator Case<TValue, TOut>((TValue, TOut) pair)
		{
			return new Case<TValue, TOut>(pair.Item2, pair.Item1);
		}

		public override string ToString()
		{
			return string.Join(", ", Arr.Select(x => x?.ToString() ?? "null"));
		}
	}
}

using System.Collections;
using System.Collections.Generic;

namespace Utils
{
	public class LazyList<T> : IEnumerable<T>
	{
		private readonly List<T> enumerated = new List<T>();

		private readonly IEnumerable<T> enumerable;

		protected LazyList(IEnumerable<T> enumerable)
		{
			this.enumerable = enumerable;
		}

		public static LazyList<T> Create(IEnumerable<T> enumerable)
		{
			switch (enumerable)
			{
				case null:
					return null;

				case LazyList<T> list:
					return list;

				default:
					return new LazyList<T>(enumerable);
			}
		}

		public IEnumerator<T> GetEnumerator() => new LazyEnumerator<T>(enumerable, enumerated);

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	public class LazyList : IEnumerable
	{
		private readonly ArrayList enumerated = new ArrayList();

		private readonly IEnumerable enumerable;

		protected LazyList(IEnumerable enumerable)
		{
			this.enumerable = enumerable;
		}

		public static LazyList Create(IEnumerable enumerable)
		{
			if (enumerable is LazyList list)
			{
				return list;
			}
			else
			{
				return new LazyList(enumerable);
			}
		}

		public static LazyList<T> Create<T>(IEnumerable<T> enumerable) => LazyList<T>.Create(enumerable);

		public IEnumerator GetEnumerator() => new LazyEnumerator(enumerable, enumerated);
	}
}
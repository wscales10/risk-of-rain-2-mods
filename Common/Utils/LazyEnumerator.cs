using System.Collections;
using System.Collections.Generic;

namespace Utils
{
	public class LazyEnumerator : IEnumerator
	{
		private readonly IEnumerable enumerable;

		private readonly ArrayList enumerated;

		private IEnumerator enumerator1;

		private bool got;

		private bool useList;

		private object current;

		private IEnumerator enumerator2;

		public LazyEnumerator(IEnumerable enumerable, ArrayList enumerated)
		{
			this.enumerable = enumerable;
			this.enumerated = enumerated;
			Reset();
		}

		public object Current
		{
			get
			{
				if (useList)
				{
					return enumerator2.Current;
				}

				if (got)
				{
					return current;
				}
				else
				{
					enumerated.Add(current = enumerator1.Current);
					got = true;
					return current;
				}
			}
		}

		public bool MoveNext()
		{
			got = false;

			if (useList)
			{
				useList = enumerator2.MoveNext();
			}

			return enumerator1.MoveNext();
		}

		public void Reset()
		{
			useList = true;
			got = false;
			current = default;
			enumerator1 = enumerable.GetEnumerator();
			enumerator2 = enumerated.GetEnumerator();
		}
	}

	public class LazyEnumerator<T> : IEnumerator<T>
	{
		private readonly IEnumerable<T> enumerable;

		private readonly List<T> enumerated;

		private IEnumerator<T> enumerator1;

		private List<T>.Enumerator enumerator2;

		private bool got;

		private bool useList;

		private T current;

		public LazyEnumerator(IEnumerable<T> enumerable, List<T> enumerated)
		{
			this.enumerable = enumerable;
			this.enumerated = enumerated;
			Reset();
		}

		public T Current
		{
			get
			{
				if (useList)
				{
					return enumerator2.Current;
				}

				if (got)
				{
					return current;
				}
				else
				{
					enumerated.Add(current = enumerator1.Current);
					got = true;
					return current;
				}
			}
		}

		object IEnumerator.Current => Current;

		public void Dispose() => enumerator1.Dispose();

		public bool MoveNext()
		{
			got = false;

			if (useList)
			{
				useList = enumerator2.MoveNext();
			}

			return enumerator1.MoveNext();
		}

		public void Reset()
		{
			useList = true;
			got = false;
			current = default;
			enumerator1 = enumerable.GetEnumerator();
			enumerator2 = enumerated.GetEnumerator();
		}
	}
}
using System;
using System.Collections;
using System.Collections.Generic;

namespace Utils
{
	public class Cache<TKey, TValue> : Cache<TKey, TValue, Dictionary<TKey, TValue>>
	{
		public Cache(Func<TKey, TValue> getter) : base(getter)
		{
		}
	}

	public class Cache<TKey, TValue, TDictionary> : IReadOnlyDictionary<TKey, TValue>
		where TDictionary : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, new()
	{
		private readonly Func<TKey, TValue> getter;

		public Cache(Func<TKey, TValue> getter) => this.getter = getter;

		public IEnumerable<TKey> Keys => ReadOnlyDictionary.Keys;

		public IEnumerable<TValue> Values => ReadOnlyDictionary.Values;

		public int Count => ReadOnlyDictionary.Count;

		protected virtual TDictionary Dictionary { get; } = new TDictionary();

		protected IReadOnlyDictionary<TKey, TValue> ReadOnlyDictionary => Dictionary;

		public virtual TValue this[TKey key]
		{
			get
			{
				if (!ReadOnlyDictionary.TryGetValue(key, out TValue value))
				{
					value = getter(key);
					((IDictionary<TKey, TValue>)Dictionary)[key] = value;
				}

				return value;
			}
		}

		public bool ContainsKey(TKey key) => ReadOnlyDictionary.ContainsKey(key);

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Dictionary.GetEnumerator();

		public bool TryGetValue(TKey key, out TValue value) => ReadOnlyDictionary.TryGetValue(key, out value);

		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Dictionary).GetEnumerator();
	}
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Utils
{
    public class Cache<TKey, TValue> : Cache<TKey, TValue, Dictionary<TKey, TValue>>
    {
        public Cache(Func<TKey, TValue> getValue, Predicate<TValue> isEmpty = null, Action<TKey> dispose = null) : base(getValue, isEmpty, dispose)
        {
        }
    }

    public class Cache<TKey, TValue, TDictionary> : IReadOnlyDictionary<TKey, TValue>
        where TDictionary : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, new()
    {
        private readonly Func<TKey, TValue> getValue;

        private readonly Predicate<TValue> isEmpty;

        private readonly Action<TKey> dispose;

        public Cache(Func<TKey, TValue> getValue, Predicate<TValue> isEmpty = null, Action<TKey> dispose = null)
        {
            this.getValue = getValue;
            this.isEmpty = isEmpty;
            this.dispose = dispose;
        }

        public IEnumerable<TKey> Keys => ReadOnlyDictionary.Keys;

        public IEnumerable<TValue> Values => ReadOnlyDictionary.Values;

        public int Count => ReadOnlyDictionary.Count;

        protected virtual TDictionary Dictionary { get; } = new TDictionary();

        protected IReadOnlyDictionary<TKey, TValue> ReadOnlyDictionary => Dictionary;

        public IEnumerable<TValue> this[IEnumerable<TKey> index] => index.Select(key => this[key]);

        public virtual TValue this[TKey key]
        {
            get
            {
                if (!ReadOnlyDictionary.TryGetValue(key, out TValue value))
                {
                    value = getValue(key);
                    ((IDictionary<TKey, TValue>)Dictionary)[key] = value;
                }

                return value;
            }
        }

        public bool TryRemove(TKey key)
        {
            if (TryGetValue(key, out TValue value) && (isEmpty?.Invoke(value) != false))
            {
                Dictionary.Remove(key);
                dispose?.Invoke(key);
                return true;
            }

            return false;
        }

        public IEnumerable<TKey> TryRemove(IEnumerable<TKey> keys) => keys.Where(TryRemove);

        public bool ContainsKey(TKey key) => ReadOnlyDictionary.ContainsKey(key);

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Dictionary.GetEnumerator();

        public bool TryGetValue(TKey key, out TValue value) => ReadOnlyDictionary.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Dictionary).GetEnumerator();
    }
}
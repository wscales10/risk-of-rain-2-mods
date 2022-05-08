﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Utils
{
    public class AsyncCache<TKey, TValue>
    {
        private readonly AsyncCache<TKey, TValue, Dictionary<TKey, TValue>> cache;

        public AsyncCache(Func<TKey, Task<ConditionalValue<TValue>>> getter)
        {
            cache = new AsyncCache<TKey, TValue, Dictionary<TKey, TValue>>(getter);
        }

        public Task GetValueAsync(TKey key, Action<TValue> delayedSetter)
        {
            var task = cache[key];
            return task.ContinueWith(t => delayedSetter(t.Result), TaskScheduler.Default);
        }
    }

    public class AsyncCache<TKey, TValue, TDictionary> : IReadOnlyDictionary<TKey, Task<TValue>>
        where TDictionary : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, new()
    {
        private readonly Func<TKey, Task<ConditionalValue<TValue>>> getter;

        public AsyncCache(Func<TKey, Task<ConditionalValue<TValue>>> getter) => this.getter = getter;

        public IEnumerable<TKey> Keys => ReadOnlyDictionary.Keys;

        public IEnumerable<Task<TValue>> Values => ReadOnlyDictionary.Values.Select(Task.FromResult);

        public int Count => ReadOnlyDictionary.Count;

        protected virtual TDictionary Dictionary { get; } = new TDictionary();

        protected IReadOnlyDictionary<TKey, TValue> ReadOnlyDictionary => Dictionary;

        public IEnumerable<Task<TValue>> this[IEnumerable<TKey> index] => index.Select(key => this[key]);

        public virtual Task<TValue> this[TKey key]
        {
            get
            {
                if (!ReadOnlyDictionary.TryGetValue(key, out TValue value))
                {
                    return GetValueAsync(key);
                }

                return Task.FromResult(value);
            }
        }

        public bool ContainsKey(TKey key) => ReadOnlyDictionary.ContainsKey(key);

        public IEnumerator<KeyValuePair<TKey, Task<TValue>>> GetEnumerator() => Keys.Zip(Values, (k, v) => new KeyValuePair<TKey, Task<TValue>>(k, v)).GetEnumerator();

        public bool TryGetValue(TKey key, out Task<TValue> value)
        {
            if (ReadOnlyDictionary.TryGetValue(key, out var result))
            {
                value = Task.FromResult(result);
                return true;
            }
            else
            {
                value = Task.FromResult(default(TValue));
                return false;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private async Task<TValue> GetValueAsync(TKey key)
        {
            var value = await getter(key);

            if (value.HasValue)
            {
                ((IDictionary<TKey, TValue>)Dictionary)[key] = value.Value;
            }

            return value.Value;
        }
    }

    public class ConditionalValue<T>
    {
        public ConditionalValue(T value) : this(true, value)
        {
        }

        public ConditionalValue()
        {
        }

        public ConditionalValue(bool hasValue, T value)
        {
            HasValue = hasValue;
            Value = value;
        }

        public bool HasValue { get; }

        public T Value { get; }
    }
}
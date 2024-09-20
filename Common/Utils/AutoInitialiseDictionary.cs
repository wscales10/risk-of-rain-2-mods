using System;

namespace Utils
{
    /// <summary>
    /// Cache which creates new values using the default constructor
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    public class AutoInitialiseDictionary<TKey, TValue> : Cache<TKey, TValue>
        where TValue : new()
    {
        public AutoInitialiseDictionary(Predicate<TValue> isEmpty = null, Action<TKey> dispose = null) : base(_ => new TValue(), isEmpty, dispose)
        {
        }
    }
}
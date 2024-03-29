﻿using System;

namespace Utils
{
    public class AutoInitialiseDictionary<TKey, TValue> : Cache<TKey, TValue>
        where TValue : new()
    {
        public AutoInitialiseDictionary(Predicate<TValue> isEmpty = null, Action<TKey> dispose = null) : base(_ => new TValue(), isEmpty, dispose)
        {
        }
    }
}
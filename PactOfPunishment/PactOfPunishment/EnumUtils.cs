using System;
using System.Collections.Generic;
using System.Linq;

namespace PactOfPunishment
{
    public static class EnumUtils
    {
        private static readonly Dictionary<Type, Array> enumValues = new Dictionary<Type, Array>();

        public static T NextEnum<T>(this Xoroshiro128Plus rng)
            where T : struct, Enum
        {
            return rng.NextElementUniform(GetValuesInternal<T>());
        }

        public static T Random<T>()
            where T : struct, Enum
        {
            var values = GetValuesInternal<T>();
            return values[UnityEngine.Random.Range(0, values.Length)];
        }

        public static T[] GetValues<T>()
            where T : struct, Enum
        {
            return GetValuesInternal<T>().ToArray();
        }

        private static T[] GetValuesInternal<T>()
            where T : struct, Enum
        {
            T[] output;

            if (enumValues.TryGetValue(typeof(T), out var values))
            {
                output = (T[])values;
            }
            else
            {
                output = Enum.GetValues(typeof(T)).Cast<T>().ToArray();
                enumValues.Add(typeof(T), output);
            }

            return output;
        }
    }
}
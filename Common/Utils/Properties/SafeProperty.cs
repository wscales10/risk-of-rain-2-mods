using System;

namespace Utils.Properties
{
    internal static class SafeProperty
    {
        internal static bool AmbiguousEquals<T>(T value1, T value2)
        {
            if (typeof(IEquatable<T>).IsAssignableFrom(typeof(T)))
            {
                return value1 as IEquatable<T> == value2 as IEquatable<T>;
            }
            else
            {
                return object.Equals(value1, value2);
            }
        }

        internal static bool Equals<T>(IEquatable<T> value1, IEquatable<T> value2)
        {
            return value1?.Equals(value2) ?? value2 == null;
        }

        internal static bool Equals<T>(T value1, T value2)
            where T : IEquatable<T>
        {
            return value1?.Equals(value2) ?? value2 == null;
        }

        internal static bool Equals<T>(T? nullable1, T? nullable2)
            where T : struct, IEquatable<T>
        {
            return nullable1 is T value1 ? nullable2 is T value2 && value1.Equals(value2) : nullable2 is null;
        }
    }
}
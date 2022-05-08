using System;
using Utils.Properties;

namespace Utils
{
    public interface ISafeProperty<T>
    {
        T Get();
    }

    public abstract class SafePropertyBase<T> : IEquatable<SafePropertyBase<T>>, ISafeProperty<T>
    {
        protected T fieldValue;

        protected SafePropertyBase()
        { }

        protected SafePropertyBase(T fieldValue)
        {
            this.fieldValue = fieldValue;
        }

        public static implicit operator T(SafePropertyBase<T> prop) => prop.Get();

        public static bool operator ==(SafePropertyBase<T> p1, SafePropertyBase<T> p2) => p1 is null ? p2 is null : p1.Equals(p2);

        public static bool operator !=(SafePropertyBase<T> p1, SafePropertyBase<T> p2) => !(p1 == p2);

        public T Get() => fieldValue;

        public override string ToString() => fieldValue.ToString();

        public override bool Equals(object o) => Equals(o as SafePropertyBase<T>);

        public bool Equals(SafePropertyBase<T> p)
        {
            if (p is null)
            {
                return false;
            }

            if (ReferenceEquals(this, p))
            {
                return true;
            }

            if (GetType() != p.GetType())
            {
                return false;
            }

            return SafeProperty.AmbiguousEquals(fieldValue, p.fieldValue);
        }

        public override int GetHashCode() => fieldValue.GetHashCode();
    }
}
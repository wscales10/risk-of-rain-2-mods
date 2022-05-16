using Patterns.Patterns;
using Patterns.TypeDefs;
using System;

namespace Patterns
{
    public delegate IPattern<T> GenericEqualizer<T>(T value);

    public delegate IPattern TypedEqualizer(Type type, object value);

    public delegate IPattern UntypedEqualizer(object value);

    public delegate IPattern Definer(string s);

    public abstract class Pattern<T> : PatternBase, IPattern<T>
    {
        public Type ValueType => typeof(T);

        protected bool IsNullableEnumPattern => IsNullableEnumType(ValueType);

        public static OrPattern<T> operator |(Pattern<T> p1, IPattern<T> p2) => new OrPattern<T>(p1, p2);

        public static AndPattern<T> operator &(Pattern<T> p1, IPattern<T> p2) => new AndPattern<T>(p1, p2);

        public static Pattern<T> operator !(Pattern<T> p) => new NotPattern<T>(p);

        public override bool IsMatch(object obj)
        {
            T value;

            try
            {
                value = (T)obj;
            }
            catch (InvalidCastException)
            {
                return false;
            }
            catch (NullReferenceException)
            {
                return false;
            }

            return IsMatch(value);
        }

        public abstract bool IsMatch(T value);

        public virtual IPattern<T> Simplify() => this;
    }
}
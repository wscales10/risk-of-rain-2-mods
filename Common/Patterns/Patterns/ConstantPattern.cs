using System;
using System.Xml.Linq;

namespace Patterns.Patterns
{
	public sealed class ConstantPattern<T> : Pattern<T>, IEquatable<ConstantPattern<T>>, INotSimplifiable<T>
	{
		private readonly bool isMatch;

		private ConstantPattern(bool isMatch)
		{
			this.isMatch = isMatch;
		}

		public override bool IsMatch(T value) => isMatch;

		public override XElement ToXml() => new XElement(isMatch.ToString());

		public static ConstantPattern<T> True { get; } = new ConstantPattern<T>(true);

		public static ConstantPattern<T> False { get; } = new ConstantPattern<T>(false);

		public static bool operator true(ConstantPattern<T> p) => p.isMatch;

		public static bool operator false(ConstantPattern<T> p) => !p.isMatch;

		public static ConstantPattern<T> operator !(ConstantPattern<T> cp) => cp ? False : True;

		public static IPattern<T> operator |(IPattern<T> p, ConstantPattern<T> cp) => cp ? cp : p;

		public static IPattern<T> operator &(IPattern<T> p, ConstantPattern<T> cp) => cp ? p : cp;

		public static IPattern<T> operator |(ConstantPattern<T> cp, IPattern<T> p) => p | cp;

		public static IPattern<T> operator &(ConstantPattern<T> cp, IPattern<T> p) => p & cp;

		public override bool Equals(object o) => Equals(o as ConstantPattern<T>);

		public bool Equals(ConstantPattern<T> cp)
		{
			if (cp is null)
			{
				return false;
			}

			if (ReferenceEquals(this, cp))
			{
				return true;
			}

			if (GetType() != cp.GetType())
			{
				return false;
			}

			return isMatch == cp.isMatch;
		}

		public override int GetHashCode() => isMatch.GetHashCode();

		public IPattern<T> SimplifyNot() => !this;

		public static bool operator ==(ConstantPattern<T> cp1, ConstantPattern<T> cp2) => cp1 is null ? cp2 is null : cp1.Equals(cp2);

		public static bool operator !=(ConstantPattern<T> cp1, ConstantPattern<T> cp2) => !(cp1 == cp2);

		public override string ToString() => isMatch.ToString();

		public override IPattern Correct()
		{
			if (IsNullableEnumPattern)
			{
				return new ConstantPattern<Enum>(isMatch);
			}
			else
			{
				return this;
			}
		}
	}
}

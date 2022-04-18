using System;
using System.Xml.Linq;
using Utils;

namespace Patterns.Patterns
{
	public class NotPattern<T> : Pattern<T>, INotSimplifiable<T>
	{
		private readonly IPattern<T> p;

		public NotPattern(IPattern<T> p)
		{
			this.p = p;
		}

		public NotPattern()
		{
		}

		public static IPattern<T> operator !(NotPattern<T> p) => p.p;

		public static NotPattern<T> Parse(XElement element, PatternParser patternParser)
		{
			return new NotPattern<T>(patternParser.Parse<T>(element.OnlyChild()));
		}

		public override bool IsMatch(T value) => !IsMatch(value);

		public override XElement ToXml() => new XElement("Not", p.ToXml());

		public override string ToString() => $"Not({p})";

		public override IPattern<T> Simplify()
		{
			var p = this.p.Simplify();

			if (p is INotSimplifiable<T> ns)
			{
				return ns.SimplifyNot() ?? p;
			}

			return new NotPattern<T>(p);
		}

		public IPattern<T> SimplifyNot() => !this;

		public override IPattern Correct()
		{
			if (IsNullableEnumPattern)
			{
				return new NotPattern<Enum>((IPattern<Enum>)p.Correct());
			}
			else
			{
				return this;
			}
		}
	}
}
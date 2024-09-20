using System;
using System.Xml.Linq;
using Utils;

namespace Patterns.Patterns
{
    public class NotPattern<T> : Pattern<T>, INotSimplifiable<T>, IOnlyChildPattern<T>
    {
        public NotPattern(IPattern<T> p)
        {
            Child = p;
        }

        public NotPattern()
        {
        }

        public IPattern<T> Child { get; set; }

        public static IPattern<T> operator !(NotPattern<T> p) => p.Child;

        public static NotPattern<T> Parse(XElement element, PatternParser patternParser)
        {
            return new NotPattern<T>(patternParser.Parse<T>(element.OnlyChild()));
        }

        public override bool IsMatch(T value) => !Child.IsMatch(value);

        public override XElement ToXml() => new XElement("Not", Child.ToXml());

        public override string ToString() => $"Not({Child})";

        public override IPattern<T> Simplify()
        {
            if (this.Child is null)
            {
                return this;
            }

            var p = this.Child.Simplify();

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
                return new NotPattern<Enum>((IPattern<Enum>)Child.Correct());
            }
            else
            {
                return this;
            }
        }
    }
}
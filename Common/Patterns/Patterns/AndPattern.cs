using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Patterns.Patterns
{
	public class AndPattern<T> : Pattern<T>, IAndSimplifiable<T>, IListPattern
	{
		public AndPattern(params IPattern<T>[] patterns) : this((IList<IPattern<T>>)patterns)
		{
		}

		public AndPattern(IList<IPattern<T>> patterns)
		{
			Children = patterns.Flatten<AndPattern<T>, IPattern<T>>(ap => ap.Children);
		}

		public AndPattern() : this(new IPattern<T>[] { })
		{
		}

		public List<IPattern<T>> Children { get; }

		IList IListPattern.Children => Children;

		public static AndPattern<T> Parse(XElement element, PatternParser patternParser)
		{
			return new AndPattern<T>(element.Elements().Select(patternParser.Parse<T>).ToArray());
		}

		public override bool IsMatch(T value) => Children.All(p => p.IsMatch(value));

		public override XElement ToXml()
		{
			return new XElement("And", Children.Select(p => p.ToXml()).ToArray());
		}

		public override string ToString() => $"And({string.Join(", ", Children)})";

		public override IPattern<T> Simplify()
		{
			var newChildren = Children.ConvertAll(p => p.Simplify());
			newChildren.RemoveAll(p => p as ConstantPattern<T> == ConstantPattern<T>.True);

			if (newChildren.Any(p => p as ConstantPattern<T> == ConstantPattern<T>.False))
			{
				return ConstantPattern<T>.False;
			}

			newChildren = HelperMethods.Simplify(newChildren, SimplifyAnd);

			switch (newChildren.Count)
			{
				case 0:
					return ConstantPattern<T>.False;

				case 1:
					return newChildren[0];

				default:
					return new AndPattern<T>(newChildren);
			}
		}

		public IPattern<T> SimplifyAnd(IPattern<T> other)
		{
			var list = Children.ToList();

			if (other is AndPattern<T> ap)
			{
				list.AddRange(ap.Children);
			}
			else
			{
				list.Add(other);
			}

			return new AndPattern<T>(list);
		}

		public override IPattern Correct()
		{
			if (IsNullableEnumPattern)
			{
				return new AndPattern<Enum>(Children.Select(c => c.Correct()).Cast<IPattern<Enum>>().ToArray());
			}
			else
			{
				return this;
			}
		}

		private static IPattern<T> SimplifyAnd(IPattern<T> one, IPattern<T> two)
		{
			if (!(one is IAndSimplifiable<T> ias))
			{
				return null;
			}

			return ias.SimplifyAnd(two);
		}
	}
}
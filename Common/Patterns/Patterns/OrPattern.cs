using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Patterns.Patterns
{
	public class OrPattern<T> : Pattern<T>, IOrSimplifiable<T>, IListPattern
	{
		public OrPattern(params IPattern<T>[] patterns) : this((IList<IPattern<T>>)patterns)
		{
		}

		public OrPattern(IEnumerable<IPattern<T>> patterns)
		{
			Children = patterns.Flatten<OrPattern<T>, IPattern<T>>(op => op.Children);
		}

		public OrPattern() : this(new IPattern<T>[] { })
		{
		}

		public List<IPattern<T>> Children { get; }

		IList IListPattern.Children => Children;

		public static OrPattern<T> Parse(XElement element, PatternParser patternParser)
		{
			return new OrPattern<T>(element.Elements().Select(patternParser.Parse<T>).ToArray());
		}

		public override bool IsMatch(T value) => Children.Any(p => p.IsMatch(value));

		public override XElement ToXml() => new XElement("Or", Children.Select(p => p.ToXml()).ToArray());

		public override string ToString() => $"Or({string.Join(", ", Children)})";

		public override IPattern<T> Simplify()
		{
			var newChildren = Children.ConvertAll(p => p.Simplify());
			newChildren.RemoveAll(p => p as ConstantPattern<T> == ConstantPattern<T>.False);

			if (newChildren.Any(p => p as ConstantPattern<T> == ConstantPattern<T>.True))
			{
				return ConstantPattern<T>.True;
			}

			newChildren = HelperMethods.Simplify(newChildren, SimplifyOr);

			switch (newChildren.Count)
			{
				case 0:
					return ConstantPattern<T>.False;

				case 1:
					return newChildren[0];

				default:
					return new OrPattern<T>(newChildren);
			}
		}

		public IPattern<T> SimplifyOr(IPattern<T> other)
		{
			var list = Children.ToList();

			if (other is OrPattern<T> op)
			{
				list.AddRange(op.Children);
			}
			else
			{
				list.Add(other);
			}

			return new OrPattern<T>(list);
		}

		public override IPattern Correct()
		{
			if (IsNullableEnumPattern)
			{
				return new OrPattern<Enum>(Children.Select(c => c.Correct()).Cast<IPattern<Enum>>().ToArray());
			}
			else
			{
				return this;
			}
		}

		private static IPattern<T> SimplifyOr(IPattern<T> one, IPattern<T> two)
		{
			if (!(one is IOrSimplifiable<T> ios))
			{
				return null;
			}

			return ios.SimplifyOr(two);
		}
	}
}
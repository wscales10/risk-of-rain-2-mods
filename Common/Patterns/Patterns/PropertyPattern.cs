using System;
using System.Xml.Linq;
using Utils;

namespace Patterns.Patterns
{
	public class PropertyPattern<TObject> : Pattern<TObject>
	{
		private PropertyPattern(PropertyInfo propertyInfo, IPattern pattern)
		{
			PropertyInfo = propertyInfo;
			Pattern = pattern;
		}

		protected PropertyPattern(string propertyName, Type propertyType, IPattern pattern) : this(new PropertyInfo(propertyName, propertyType), pattern)
		{
			PropertyInfo = new PropertyInfo(propertyName, propertyType);
			Pattern = pattern;
		}

		public PropertyInfo PropertyInfo { get; }

		public IPattern Pattern { get; }

		public override XElement ToXml()
		{
			var element = new XElement("Property", Pattern.Correct().ToXml());
			PropertyInfo.AddAttributesTo(element);
			return element;
		}

		public static PropertyPattern<TObject> Parse(XElement element, PatternParser patternParser)
		{
			var propertyInfo = PropertyInfo.Parse(element, patternParser);
			return new PropertyPattern<TObject>(propertyInfo, patternParser.Parse(propertyInfo.Type, element.OnlyChild()));
		}

		public static PropertyPattern<TObject> Create<T>(string propertyName, IPattern pattern)
		{
			return new PropertyPattern<TObject>(propertyName, typeof(T), pattern);
		}

		public static PropertyPattern<TObject> Create<T>(string propertyName, IPattern<T> pattern)
		{
			return new PropertyPattern<TObject>(propertyName, typeof(T), pattern.Simplify());
		}

		public override bool IsMatch(TObject value)
		{
			return Pattern.IsMatch(value.GetPropertyValue(PropertyInfo.Name));
		}

		public override string ToString() => $".{PropertyInfo.Name} matches {Pattern}";
	}
}

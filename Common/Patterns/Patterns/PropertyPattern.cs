using System;
using System.Xml.Linq;
using Utils;
using Utils.Reflection.Properties;

namespace Patterns.Patterns
{
	public class PropertyPattern<TObject> : Pattern<TObject>
	{
		internal PropertyPattern(string propertyName, Type propertyType, IPattern pattern) : this(new PropertyInfo(propertyName, propertyType), pattern)
		{
			PropertyInfo = new PropertyInfo(propertyName, propertyType);
			Pattern = pattern;
		}

		private PropertyPattern(PropertyInfo propertyInfo = null, IPattern pattern = null)
		{
			PropertyInfo = propertyInfo;
			Pattern = pattern;
		}

		public PropertyInfo PropertyInfo { get; }

		public IPattern Pattern { get; }

		public static PropertyPattern<TObject> Parse(XElement element, PatternParser patternParser)
		{
			if (element is null)
			{
				throw new ArgumentNullException(nameof(element));
			}

			if (patternParser is null)
			{
				throw new ArgumentNullException(nameof(patternParser));
			}

			var propertyInfo = PropertyInfo.Parse<TObject>(element);

			if (propertyInfo is null)
			{
				throw new InvalidOperationException($"{nameof(propertyInfo)} cannot be null");
			}

			if (propertyInfo.Type is null)
			{
				throw new InvalidOperationException($"{nameof(propertyInfo)}.{nameof(propertyInfo.Type)} cannot be null");
			}

			return new PropertyPattern<TObject>(propertyInfo, patternParser.Parse(propertyInfo.Type, element.OnlyChild()));
		}

		public override XElement ToXml()
		{
			var element = new XElement("Property", Pattern.ToXml());
			PropertyInfo.AddAttributesTo(element);
			return element;
		}

		public override bool IsMatch(TObject value)
		{
			return Pattern.IsMatch(value.GetPropertyValue(PropertyInfo.Name));
		}

		public override string ToString() => $"{PropertyInfo}: {Pattern}";
	}
}
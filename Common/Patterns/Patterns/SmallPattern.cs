using System;
using System.Linq;
using System.Xml.Linq;

namespace Patterns.Patterns
{
	public abstract class SmallPattern<T> : Pattern<T>
	{
		public abstract string Definition { get; }

		public sealed override string ToString() => Definition;

		public override XElement ToXml()
		{
			if(Definition is null)
			{
				throw new NullReferenceException();
			}

			var element = new XElement("Pattern", Definition);
			element.SetAttributeValue("type", TypeKey);

			if (typeof(T).IsGenericType)
			{
				element.SetAttributeValue("of", GetTypeDefKey(typeof(T).GenericTypeArguments.Single()));
			}

			return element;
		}

		public virtual string TypeKey => GetTypeDefKey(typeof(T));
	}
}

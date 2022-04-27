using System;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using Utils;

namespace Patterns.Patterns
{
	public class PropertyInfo : IEquatable<PropertyInfo>
	{
		private static readonly Cache<Type, LazyList<PropertyInfo>> cache = new Cache<Type, LazyList<PropertyInfo>>(t => LazyList.Create(t.GetProperties().Select(p => new PropertyInfo(p.Name, p.PropertyType))));

		public PropertyInfo(string name, Type type)
		{
			Name = name;
			Type = type;
		}

		public string Name { get; }

		public Type Type { get; }

		public (string typeKey, string genericTypeKey) TypeDefKeys => PatternBase.GetTypeDefKeys(Type);

		public static PropertyInfo Parse<TObject>(XElement element)
		{
			string name = element.Attribute("name").Value;
			return cache[typeof(TObject)].SingleOrDefault(p => p.Name == name);
		}

		public static bool operator ==(PropertyInfo info1, PropertyInfo info2) => info1 is null ? info2 is null : info1.Equals(info2);

		public static bool operator !=(PropertyInfo info1, PropertyInfo info2) => !(info1 == info2);

		public void AddAttributesTo(XElement element)
		{
			/*var (typeKey, genericTypeKey) = TypeDefKeys;
			element.SetAttributeValue("type", typeKey);

			if (!(genericTypeKey is null))
			{
				element.SetAttributeValue("of", genericTypeKey);
			}*/

			element.SetAttributeValue("name", Name);
		}

		public PropertyInfo Correct()
		{
			return typeof(Enum).IsAssignableFrom(Type) ? new PropertyInfo(Name, typeof(Enum)) : this;
		}

		public override string ToString() => Name;

		public override bool Equals(object o) => Equals(o as PropertyInfo);

		public bool Equals(PropertyInfo info)
		{
			if (info is null)
			{
				return false;
			}

			if (ReferenceEquals(this, info))
			{
				return true;
			}

			if (GetType() != info.GetType())
			{
				return false;
			}

			return Name == info.Name && Type == info.Type;
		}

		public override int GetHashCode() => Name.GetHashCode();
	}
}
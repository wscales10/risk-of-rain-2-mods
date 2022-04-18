using System;
using System.Linq;
using System.Xml.Linq;
using Utils;

namespace Patterns
{
	public abstract class PatternBase : IPattern
	{
		public abstract bool IsMatch(object value);

		public abstract XElement ToXml();

		internal static string GetTypeDefKey(Type type)
		{
			if(type is null)
			{
				return null;
			}

			if (typeof(Enum).IsAssignableFrom(type) && typeof(Enum) != type)
			{
				return nameof(Enum) + "`1";
			}

			return type.Name;
		}

		protected static bool IsNullableEnumType(Type type) => type.IsGenericType(typeof(Nullable<>)) && typeof(Enum).IsAssignableFrom(type.GenericTypeArguments.Single());

		internal static (string, string) GetTypeDefKeys(Type type)
		{
			if (IsNullableEnumType(type))
			{
				return GetTypeDefKeys(typeof(Enum));
			}
			else
			{
				return (GetTypeDefKey(type), GetTypeDefKey(type.GenericTypeArguments.SingleOrDefault()));
			}
		}

		public virtual IPattern Correct() => this;
	}
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Utils
{
	public static class ExtensionMethods
	{
		private static readonly Regex genericTypeNameInfoRegex = new Regex(@"`\d+$");

		public static T AsEnum<T>(this string value, bool ignoreCase = false)
			where T : struct, Enum
		{
			return (T)Enum.Parse(typeof(T), value, ignoreCase);
		}

		public static T AsEnum<T>(this Enum value)
			where T : struct, Enum
		{
			return value.ToString().AsEnum<T>();
		}

		public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this IList<T> source) => new ReadOnlyCollection<T>(source);

		public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> source) => source.ToList().ToReadOnlyCollection();

		public static Dictionary<int, T> ToDictionary<T>(this IList<T> list)
		{
			var output = new Dictionary<int, T>();

			for (int i = 0; i < list.Count; i++)
			{
				output[i] = list[i];
			}

			return output;
		}

		public static ReadOnlyDictionary<TKey, TValue> ToReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> mutable)
		{
			return new ReadOnlyDictionary<TKey, TValue>(mutable);
		}

		public static string ToCompactString(this TimeSpan timeSpan)
		{
			string firstPart = timeSpan < TimeSpan.Zero ? "-" : string.Empty;
			string secondPart = timeSpan.Milliseconds != 0 ? timeSpan.ToString(@"\.FFF") : string.Empty;

			if (timeSpan.Hours != 0)
			{
				firstPart += timeSpan.ToString(@"h\:mm\:ss");
			}
			else if (timeSpan.Minutes != 0)
			{
				firstPart += timeSpan.ToString(@"mm\:ss");
			}
			else
			{
				firstPart += timeSpan.ToString("%s");
				secondPart += "s";
			}

			return firstPart + secondPart;
		}

		public static XElement OnlyChild(this XElement element, bool allowNull = false) => allowNull ? element.Elements().SingleOrDefault() : element.Elements().Single();

		public static bool IsGenericType(this Type type, Type genericTypeDefinition)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition() == genericTypeDefinition;
		}

		public static string GetDisplayName(this Type type, bool withGenericTypeArguments = true)
		{
			string output = type.Name;

			if (!type.IsGenericType)
			{
				return output;
			}

			if (type.IsGenericType(typeof(Nullable<>)) && !(type.GenericTypeArguments.SingleOrDefault() is null))
			{
				return $"{type.GenericTypeArguments[0].GetDisplayName(withGenericTypeArguments)}?";
			}

			output = genericTypeNameInfoRegex.Replace(output, string.Empty);

			if (!withGenericTypeArguments)
			{
				return output;
			}

			output += $"<{string.Join(", ", type.GenericTypeArguments.Select(t => t?.GetDisplayName()))}>";
			return output;
		}

		internal static bool SafeInvoke<T>(this Predicate<T> predicate, T obj) => predicate?.Invoke(obj) ?? true;
	}
}
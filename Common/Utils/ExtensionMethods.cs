using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Utils
{
	public static class ExtensionMethods
	{
		private static readonly Regex genericTypeNameInfoRegex = new Regex(@"`\d+$");

		public static Type Denullabled(this Type type, Type[] genericTypeArguments = null) => (type?.IsGenericType(typeof(Nullable<>)) ?? false) ? (genericTypeArguments ?? type.GenericTypeArguments).SingleOrDefault() : type;

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

		public static XElement OnlyChild(this XElement element, bool allowNull = false) => allowNull ? element.Elements().SingleOrDefault() : element.Elements().Single();

		public static bool IsGenericType(this Type type, Type genericTypeDefinition)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition() == genericTypeDefinition;
		}

		public static void LogPropertyValue(this object obj, object value, [CallerMemberName] string propertyName = null)
		{
			obj?.Log($"{obj.GetType().GetDisplayName()} {propertyName} = {value}");
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

		public static IEnumerable<T> With<T>(this IEnumerable<T> source, params T[] items) => source.Concat(items);

		public static int InsertRange<T>(this IList<T> source, int startingIndex, IEnumerable<T> collection)
		{
			int i = startingIndex;

			foreach (var item in collection)
			{
				source.Insert(i++, item);
			}

			return i;
		}

		public static void RemoveRange(this IList source, int index, int count)
		{
			for (int i = 0; i < count; i++)
			{
				source.RemoveAt(index);
			}
		}

		public static int ReplaceRange<T>(this IList<T> source, int startingIndex, IEnumerable<T> collection)
		{
			int i = startingIndex;

			foreach (var item in collection)
			{
				source[i++] = item;
			}

			return i;
		}

		public static int ReplaceRange<T, TList>(this TList source, int startingIndex, IEnumerable<T> newList, int oldCount)
			where TList : IList<T>, IList
		{
			int i = source.ReplaceRange(startingIndex, newList.Take(oldCount));

			if (i < startingIndex + oldCount)
			{
				source.RemoveRange(i, oldCount + startingIndex - i);
			}
			else
			{
				i = source.InsertRange(i, newList.Skip(oldCount));
			}

			return i;
		}

		public static bool IsOneOf<T>(this T value, params T[] array) => array.Contains(value);

		public static int MyHashCode<T>(this T item) => typeof(T).GetProperties().Aggregate(0, (i, p) => i ^ p.GetValue(item)?.GetHashCode() ?? 0);

		internal static bool SafeInvoke<T>(this Predicate<T> predicate, T obj) => predicate?.Invoke(obj) ?? true;
	}
}
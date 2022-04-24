using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Utils.Reflection.Properties
{
	public static class Static
	{
		public static object GetStaticPropertyValue(this Type type, string propertyName)
		{
			return type.GetStaticPropertyValue(pi => pi.Name == propertyName);
		}

		public static T GetStaticPropertyValue<T>(this Type type, string propertyName)
		{
			return (T)type.GetStaticPropertyValue(propertyName);
		}

		public static object GetStaticPropertyValue(this Type type, Predicate<PropertyInfo> predicate = null)
		{
			return type.GetProperties().Single(pi => predicate.SafeInvoke(pi)).GetValue(null);
		}

		public static T GetStaticPropertyValue<T>(this Type type, Predicate<PropertyInfo> predicate = null)
		{
			return (T)type.GetStaticPropertyValue(pi => pi.PropertyType == typeof(T) && predicate.SafeInvoke(pi));
		}

		public static object GetStaticPropertyValue(this Type type, Predicate<object> predicate)
		{
			return type.GetProperties().Select(pi => pi.GetValue(null)).SingleOrDefault(predicate.SafeInvoke);
		}

		public static T GetStaticPropertyValue<T>(this Type type, Predicate<T> predicate)
		{
			return type.GetProperties().Where(pi => pi.PropertyType == typeof(T)).Select(pi => pi.GetValue(null)).Cast<T>().SingleOrDefault(predicate.SafeInvoke);
		}

		public static IEnumerable GetStaticPropertyValues(this Type type, string propertyName)
		{
			return type.GetStaticPropertyValues(pi => pi.Name == propertyName);
		}

		public static IEnumerable<T> GetStaticPropertyValues<T>(this Type type, string propertyName)
		{
			return type.GetStaticPropertyValues(propertyName).Cast<T>();
		}

		public static IEnumerable GetStaticPropertyValues(this Type type, Predicate<PropertyInfo> predicate = null)
		{
			return type.GetProperties().Where(pi => predicate.SafeInvoke(pi)).Select(pi => pi.GetValue(null));
		}

		public static IEnumerable<T> GetStaticPropertyValues<T>(this Type type, Predicate<PropertyInfo> predicate = null)
		{
			return type.GetStaticPropertyValues(pi => pi.PropertyType == typeof(T) && predicate.SafeInvoke(pi)).Cast<T>();
		}

		public static IEnumerable GetStaticPropertyValues(this Type type, Predicate<object> predicate)
		{
			return type.GetProperties().Select(pi => pi.GetValue(null)).Where(predicate.SafeInvoke);
		}

		public static IEnumerable<T> GetStaticPropertyValues<T>(this Type type, Predicate<T> predicate)
		{
			return type.GetProperties().Where(pi => pi.PropertyType == typeof(T)).Select(pi => pi.GetValue(null)).Cast<T>().Where(predicate.SafeInvoke);
		}
	}
}
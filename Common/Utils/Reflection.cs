using System;
using System.Linq;
using System.Reflection;

namespace Utils
{
	public static class Reflection
	{
		public static object GetPropertyValue(this object obj, string propertyName)
		{
			return obj?.GetType().GetProperty(propertyName).GetValue(obj);
		}

		public static T GetPropertyValue<T>(this object obj, string propertyName)
		{
			return (T)obj?.GetPropertyValue(propertyName);
		}

		public static object GetStaticPropertyValue(this Type type, string propertyName)
		{
			return type.GetProperty(propertyName).GetValue(null);
		}

		public static T GetStaticPropertyValue<T>(this Type type, string propertyName)
		{
			return (T)type.GetStaticPropertyValue(propertyName);
		}

		public static object GetStaticPropertyValue(this Type type, Predicate<PropertyInfo> predicate)
		{
			return type.GetProperties().Single(pi => predicate(pi)).GetValue(null);
		}

		public static T GetStaticPropertyValue<T>(this Type type, Predicate<PropertyInfo> predicate)
		{
			return (T)type.GetStaticPropertyValue(pi => pi.PropertyType == typeof(T) && predicate(pi));
		}

		public static object GetStaticPropertyValue(this Type type, Predicate<object> predicate)
		{
			return type.GetProperties().Select(pi => pi.GetValue(null)).SingleOrDefault(x => predicate(x));
		}

		public static T GetStaticPropertyValue<T>(this Type type, Predicate<T> predicate)
		{
			return type.GetProperties().Where(pi => pi.PropertyType == typeof(T)).Select(pi => pi.GetValue(null)).Cast<T>().SingleOrDefault(x => predicate(x));
		}

		public static void SetPropertyValue(this object obj, string propertyName, object value)
		{
			obj.GetType().GetProperty(propertyName).SetValue(obj, value);
		}

		public static MethodInfo GetMethod(this Type type, string methodName, Predicate<MethodInfo> predicate)
		{
			return type.GetMethods().Single(si => si.Name == methodName && predicate(si));
		}

		public static object InvokeStatic(this MethodInfo methodInfo, params object[] parameters)
		{
			return methodInfo.Invoke(null, parameters);
		}

		public static T InvokeStatic<T>(this MethodInfo methodInfo, params object[] parameters)
		{
			return (T)methodInfo.InvokeStatic(parameters);
		}

		public static object InvokeStatic(this Type type, string methodName, params object[] parameters)
		{
			return type.GetMethod(methodName).Invoke(null, parameters);
		}

		public static T InvokeStatic<T>(this Type type, string methodName, params object[] parameters)
		{
			return (T)type.InvokeStatic(methodName, parameters);
		}

		public static ConstructorInfo GetAnyConstructor(this Type type, params Type[] argTypes)
		{
			return type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, argTypes);
		}

		public static object Construct(this Type type, params object[] parameters)
		{
			return type.GetAnyConstructor(parameters.Select(p => p.GetType()).ToArray())?.Invoke(parameters);
		}

		private static ConstructorInfo GetConstructor(this Type type, BindingFlags flags, params Type[] argTypes)
		{
			return type.GetConstructor(flags, null, argTypes, null);
		}
	}
}
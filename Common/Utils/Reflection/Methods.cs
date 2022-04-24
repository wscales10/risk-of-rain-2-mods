using System;
using System.Linq;
using System.Reflection;
using Utils;

namespace Utils.Reflection
{
	public static class Methods
	{
		public static MethodInfo GetMethod(this Type type, string methodName, Predicate<MethodInfo> predicate = null)
		{
			return type.GetMethods().Single(si => si.Name == methodName && predicate.SafeInvoke(si));
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

		public static T InvokeMethod<T>(this object obj, string methodName, params object[] parameters)
		{
			return (T)obj.GetType().GetMethod(methodName).Invoke(obj, parameters);
		}
	}
}
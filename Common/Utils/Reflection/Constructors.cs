using System;
using System.Linq;
using System.Reflection;

namespace Utils.Reflection
{
	public static class Constructors
	{
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
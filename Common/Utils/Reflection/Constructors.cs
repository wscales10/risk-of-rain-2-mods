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

		public static ConstructorInfo GetEmptyConstructor(this Type type)
		{
			return type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault(constructor =>
			{
				if (constructor.GetParameters().All(p => p.IsOptional || p.IsDefined(typeof(ParamArrayAttribute), false)))
				{
					return true;
				}

				return false;
			});
		}

		public static object ConstructDefault(this Type type)
		{
			ConstructorInfo constructor = type.GetEmptyConstructor();

			if (constructor is null)
			{
				return null;
			}

			var values = constructor.GetParameters().Select(p =>
			{
				if (p.IsOptional)
				{
					return Type.Missing;
				}

				if (p.IsDefined(typeof(ParamArrayAttribute), false))
				{
					return Array.CreateInstance(p.ParameterType.GetElementType(), 0);
				}

				throw new InvalidOperationException();
			}).ToArray();
			return constructor.Invoke(values);
		}

		private static ConstructorInfo GetConstructor(this Type type, BindingFlags flags, params Type[] argTypes)
		{
			return type.GetConstructor(flags, null, argTypes, null);
		}
	}
}
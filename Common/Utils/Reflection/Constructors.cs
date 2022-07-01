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
            foreach (var constructor in type.GetConstructors())
            {
                if (constructor.GetParameters().All(p => p.IsOptional))
                {
                    return constructor;
                }
            }

            return null;
        }

        public static object ConstructDefault(this Type type) => type.GetEmptyConstructor().Invoke(Array.Empty<object>());

        private static ConstructorInfo GetConstructor(this Type type, BindingFlags flags, params Type[] argTypes)
        {
            return type.GetConstructor(flags, null, argTypes, null);
        }
    }
}
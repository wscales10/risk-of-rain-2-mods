using System;
using System.Linq;
using System.Xml.Linq;
using Utils;

namespace Patterns
{
    public abstract class PatternBase : IPattern
    {
        public static bool Inherits(Type derivedType, Type baseType)
        {
            if (derivedType.IsGenericType(typeof(Nullable<>)))
            {
                var genericTypeArg = derivedType.GenericTypeArguments.FirstOrDefault();

                if (!(genericTypeArg is null) && typeof(Enum).IsAssignableFrom(genericTypeArg))
                {
                    return true;
                }
            }

            return baseType?.IsAssignableFrom(derivedType) ?? false;
        }

        public abstract bool IsMatch(object obj);

        public abstract XElement ToXml();

        public virtual IPattern Correct() => this;

        internal static string GetTypeDefKey(Type type)
        {
            if (type is null)
            {
                return null;
            }

            if (typeof(Enum).IsAssignableFrom(type) && typeof(Enum) != type)
            {
                return nameof(Enum) + "`1";
            }

            return type.Name;
        }

        internal static (string, string) GetTypeDefKeys(Type type)
        {
            if (false && IsNullableEnumType(type))
            {
                return GetTypeDefKeys(typeof(Enum));
            }
            else
            {
                return (GetTypeDefKey(type), GetTypeDefKey(type.GenericTypeArguments.SingleOrDefault()));
            }
        }

        protected static bool IsNullableEnumType(Type type) => type.IsGenericType(typeof(Nullable<>)) && typeof(Enum).IsAssignableFrom(type.GenericTypeArguments.Single());
    }
}
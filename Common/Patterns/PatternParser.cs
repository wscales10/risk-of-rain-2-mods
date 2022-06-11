using Patterns.Patterns;
using Patterns.Patterns.CollectionPatterns;
using Patterns.Patterns.SmallPatterns;
using Patterns.Patterns.SmallPatterns.ValuePatterns;
using Patterns.TypeDefs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Utils;
using Utils.Reflection;

namespace Patterns
{
    public class PatternParser
    {
        private readonly ReadOnlyDictionary<string, ITypeDefGetter> typeDefs;

        protected PatternParser()
        {
            typeDefs = new Dictionary<string, ITypeDefGetter>
            {
                [nameof(Int32)] = IntPattern.TypeDef,
                [nameof(Boolean)] = BoolPattern.TypeDef,
                [nameof(String)] = StringPattern.TypeDef,
                [nameof(Enum)] = EnumPattern.TypeDef,
                [nameof(Enum) + "`1"] = EnumPattern.GenericTypeDef.Get(this),
                [typeof(Nullable<>).Name] = NullablePattern.GenericTypeDef.Get(this),
                ["nullable-null"] = NullPattern.NullableTypeDef.Get(this),
                ["class-null"] = NullPattern.ClassTypeDef.Get(this),
                [typeof(IList<>).Name] = CollectionPattern.GenericTypeDef.Get(this)
            }.ToReadOnly();
        }

        public static PatternParser Instance { get; } = new PatternParser();

        public IPattern<T> DeepClone<T>(IPattern<T> pattern) => Parse<T>(pattern.ToXml());

        public IPattern DeepClone(IPattern pattern) => Parse(pattern.ValueType, pattern.ToXml());

        public IPattern Parse(Type type, XElement element)
        {
            if (!TryGetTypeDef(new TypeRef(type), out var typeDef))
            {
                throw new NotSupportedException();
            }

            return typeDef.GetParser(this)(element);
        }

        public bool TryGetTypeDef(TypeRef typeRef, out TypeDef typeDef)
        {
            var clone = typeRef.Clone();

            // typeKey and genericTypeArg
            if (!(typeRef.TypeKey is null))
            {
                if (TryGetTypeDefGetter(typeRef.TypeKey, out var typeDefGetter))
                {
                    typeDef = typeDefGetter.GetTypeDef(typeRef);
                    return true;
                }
            }

            // type only
            else if (!(typeRef.FullType is null))
            {
                clone.AssumeTypeKey();
                return TryGetTypeDef(clone, out typeDef);
            }

            typeDef = null;
            return false;
        }

        public TypeDef GetTypeDef(TypeRef typeRef)
        {
            if (!TryGetTypeDef(typeRef, out var typeDef))
            {
                throw new NotSupportedException();
            }

            return typeDef;
        }

        public Type GetType(TypeRef typeRef)
        {
            return GetTypeDef(typeRef).Type;
        }

        public Type GetType(string typeKey, string genericTypeKey = null)
        {
            return GetTypeDef(typeKey, genericTypeKey).Type;
        }

        public TypeDef GetTypeDef<T>()
        {
            return GetTypeDef(new TypeRef(typeof(T)));
        }

        /// <summary>
        /// Gets a <see cref="TypeDef"/> from two strings
        /// </summary>
        /// <param name="typeKey"></param>
        /// <param name="genericTypeKey"></param>
        /// <returns></returns>
        public TypeDef GetTypeDef(string typeKey, string genericTypeKey = null)
        {
            if (genericTypeKey is null)
            {
                return GetTypeDef(new TypeRef(typeKey));
            }
            else
            {
                return GetTypeDef(new TypeRef(typeKey, GetType(genericTypeKey)));
            }
        }

        public PropertyInfo GetPropertyInfo(XElement element)
        {
            var typeName = element.Attribute("type").Value;
            var genericTypeKey = element.Attribute("of")?.Value;
            var type = GetType(typeName, genericTypeKey);
            return new PropertyInfo(element.Attribute("name").Value, type);
        }

        public IPattern<T> GetEqualizer<T>(T value)
        {
            return (IPattern<T>)GetTypeDef<T>().Equalizer(typeof(T), value);
        }

        public IPattern<T> Parse<T>(XElement element)
        {
            switch (element.Name.ToString())
            {
                case "And":
                    return AndPattern<T>.Parse(element, this);

                case "Pattern":
                    var typeKey = element.Attribute("type").Value;
                    var genericTypeKey = element.Attribute("of")?.Value;
                    var typeRef = new TypeRef(typeof(T)) { TypeKey = typeKey, GenericTypeKeys = genericTypeKey is null ? Array.Empty<string>() : new[] { genericTypeKey } };
                    return (IPattern<T>)GetTypeDef(typeRef).Definer(element.Value);

                case "Or":
                    return OrPattern<T>.Parse(element, this);

                case "Not":
                    return NotPattern<T>.Parse(element, this);

                case "Property":
                    return PropertyPattern<T>.Parse(element, this);

                case "True":
                    return ConstantPattern<T>.True;

                case "False":
                    return ConstantPattern<T>.False;

                case "Any":
                    if (typeof(T).IsGenericType(typeof(IList<>)))
                    {
                        var genericTypeArg = typeof(T).GenericTypeArguments.Single();
                        return (IPattern<T>)typeof(AnyPattern<>).MakeGenericType(genericTypeArg).Construct(Parse(genericTypeArg, element.OnlyChild()));
                    }
                    else
                    {
                        Debugger.Break();
                        throw new XmlException();
                    }

                case "All":
                    if (typeof(T).IsGenericType(typeof(IList<>)))
                    {
                        var genericTypeArg = typeof(T).GenericTypeArguments.Single();
                        return (IPattern<T>)typeof(AllPattern<>).MakeGenericType(genericTypeArg).Construct(Parse(genericTypeArg, element.OnlyChild()));
                    }
                    else
                    {
                        Debugger.Break();
                        throw new XmlException();
                    }

                default:
                    throw new XmlException();
            }
        }

        public bool TryParse<T>(string s, out IPattern<T> output)
        {
            IPattern p = GetTypeDef<T>().Definer(s);

            if (string.IsNullOrEmpty(p?.ToString()))
            {
                output = default;
                return false;
            }
            else
            {
                output = (IPattern<T>)p;
                return true;
            }
        }

        protected virtual bool TryGetTypeDefGetter(string typeKey, out ITypeDefGetter typeDefGetter)
        {
            return typeDefs.TryGetValue(typeKey, out typeDefGetter);
        }
    }
}
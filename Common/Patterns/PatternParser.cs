using Patterns.Patterns;
using Patterns.Patterns.SmallPatterns;
using Patterns.Patterns.SmallPatterns.ValuePatterns;
using Patterns.TypeDefs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Utils;

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
				[nameof(Enum) + "`1"] = EnumPattern.GenericTypeDef,
				[typeof(Nullable<>).Name] = NullablePattern.GenericTypeDef.Get(this),
				["nullable-null"] = NullPattern.NullableTypeDef.Get(this),
				["class-null"] = NullPattern.ClassTypeDef.Get(this)
			}.ToReadOnly();
		}

		public static PatternParser Instance { get; } = new PatternParser();

		public IPattern Parse(Type type, XElement element)
		{
			if(!TryGetTypeDef(type, out var typeDef))
			{
				throw new NotSupportedException();
			}

			return typeDef.GetParser(this)(element);
		}

		public bool TryGetTypeDef(Type type, out TypeDef typeDef)
		{
			return TryGetTypeDef(type.Name, type.GenericTypeArguments.SingleOrDefault(), out typeDef);
		}

		public bool TryGetTypeDef(string typeKey, Type genericTypeArg, out TypeDef typeDef)
		{
			if(!TryGetTypeDefGetter(typeKey, out var typeDefGetter))
			{
				typeDef = null;
				return false;
			}

			if (genericTypeArg is null)
			{
				typeDef = typeDefGetter.GetTypeDef();
			}
			else
			{
				typeDef = typeDefGetter.GetTypeDef(genericTypeArg);
			}

			return true;
		}

		public Type GetType(string typeName, string genericTypeKey = null)
		{
			return GetTypeDef(typeName, genericTypeKey).Type;
		}

		public TypeDef GetTypeDef<T>()
		{
			var type = typeof(T);

			if (!TryGetTypeDef(PatternBase.GetTypeDefKey(type), type.IsGenericType ? type.GenericTypeArguments.Single() : null, out var typeDef))
			{
				throw new NotSupportedException();
			}

			return typeDef;
		}

		/// <summary>
		/// Gets a <see cref="TypeDef"/> from two strings
		/// </summary>
		/// <param name="typeKey"></param>
		/// <param name="genericTypeKey"></param>
		/// <returns></returns>
		public TypeDef GetTypeDef(string typeKey, string genericTypeKey)
		{
			if(!TryGetTypeDef(typeKey, string.IsNullOrEmpty(genericTypeKey) ? null : GetType(genericTypeKey), out var typeDef))
			{
				throw new NotSupportedException();
			}

			return typeDef;
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
					var type = element.Attribute("type").Value;
					var genericTypeKey = element.Attribute("of")?.Value;
					return (IPattern<T>)GetTypeDef(type, genericTypeKey).Definer(element.Value, typeof(T).Name, typeof(T).GenericTypeArguments.SingleOrDefault()?.Name);

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

				default:
					throw new XmlException();
			}
		}

		public bool TryParse<T>(string s, out IPattern<T> output)
		{
			var (t, gta) = PatternBase.GetTypeDefKeys(typeof(T));
			IPattern p = GetTypeDef<T>().Definer(s, t, gta);

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
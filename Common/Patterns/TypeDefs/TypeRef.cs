using System;
using System.Collections.Generic;
using Utils;

namespace Patterns.TypeDefs
{
	public class TypeRef
	{
		private Type fullType;

		public TypeRef(Type fullType)
		{
			FullType = fullType;
		}

		public TypeRef(string typeKey, params Type[] genericTypeArgs)
		{
			TypeKey = typeKey;
			GenericTypeArguments = genericTypeArgs;
		}

		public string TypeKey { get; private set; }

		public IList<string> GenericTypeKeys { get; set; } = Array.Empty<string>();

		public Type FullType
		{
			get { return fullType; }

			set
			{
				if (value.IsGenericType)
				{
					GenericTypeDef = value.GetGenericTypeDefinition();
					GenericTypeArguments = value.GenericTypeArguments.ToReadOnlyCollection();
				}

				fullType = value;
			}
		}

		public Type GenericTypeDef { get; set; }

		public IList<Type> GenericTypeArguments { get; set; } = Array.Empty<Type>();

		public int GenericSize => Math.Max(GenericTypeKeys.Count, GenericTypeArguments.Count);

		public void AssumeTypeKey()
		{
			TypeKey = PatternBase.GetTypeDefKey(fullType);
		}

		public TypeRef Clone() => (TypeRef)MemberwiseClone();
	}
}
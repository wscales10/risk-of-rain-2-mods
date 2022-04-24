using System;
using System.Linq;

namespace Patterns.TypeDefs
{
	internal class GenericArgTypeDef : ITypeDefGetter
	{
		private readonly Func<Type, TypeDef> getTypeDef;

		public GenericArgTypeDef(Func<Type, TypeDef> getTypeDef)
		{
			this.getTypeDef = getTypeDef;
		}

		public TypeDef GetTypeDef(TypeRef typeref)
		{
			return getTypeDef(typeref.GenericTypeArguments.Single());
		}
	}
}
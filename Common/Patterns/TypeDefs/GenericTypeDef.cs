using System;
using System.Linq;

namespace Patterns.TypeDefs
{
	internal class GenericTypeDef : ITypeDefGetter
	{
		private readonly Func<Type, TypeDef> getTypeDef;

		public GenericTypeDef(Func<Type, TypeDef> getTypeDef)
		{
			this.getTypeDef = getTypeDef;
		}

		public TypeDef GetTypeDef(params Type[] genericTypeArgs)
		{
			return getTypeDef(genericTypeArgs.Single());
		}
	}
}

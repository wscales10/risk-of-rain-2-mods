using System;

namespace Patterns.TypeDefs
{
	public class BestTypeDefGetter : ITypeDefGetter
	{
		private readonly Func<TypeRef, TypeDef> getTypeDef;

		public BestTypeDefGetter(Func<TypeRef, TypeDef> getTypeDef)
		{
			this.getTypeDef = getTypeDef;
		}

		public TypeDef GetTypeDef(TypeRef typeref)
		{
			return getTypeDef(typeref);
		}
	}
}
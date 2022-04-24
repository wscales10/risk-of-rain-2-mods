using System;

namespace Patterns.TypeDefs
{
	public interface ITypeDefGetter
	{
		TypeDef GetTypeDef(TypeRef typeref);
	}
}
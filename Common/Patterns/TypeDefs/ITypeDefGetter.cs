using System;

namespace Patterns.TypeDefs
{
	public interface ITypeDefGetter
	{
		TypeDef GetTypeDef(params Type[] genericTypeArgs);
	}
}

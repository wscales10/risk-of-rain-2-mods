using System;
using System.Collections;

namespace Patterns.Patterns
{
	public interface IListPattern : IPattern
	{
		IList Children { get; }

		Type ValueType { get; }
	}
}

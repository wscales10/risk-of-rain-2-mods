using System.Collections;

namespace Patterns.Patterns
{
	public interface IListPattern : IPattern
	{
		IList Children { get; }
	}
}
using System.Collections.Generic;

namespace Rules.RuleTypes.Interfaces
{
	public interface IMultiRule<T>
	{
		IEnumerable<(T expectedValue, IRule rule)> Pairs { get; }

		string PropertyName { get; }
	}
}

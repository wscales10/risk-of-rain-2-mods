using System.Collections.Generic;

namespace Rules.RuleTypes.Interfaces
{
	public interface IArrayRule : IRule
	{
		IEnumerable<IRule> Rules { get; }
	}
}

using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Mutable;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Utils;

namespace Rules.RuleTypes.Readonly
{
	public class ReadOnlyArrayRule : ReadOnlyRule<ArrayRule>, IArrayRule
	{
		public ReadOnlyArrayRule(ArrayRule arrayRule) : base(arrayRule)
		{
			Rules = arrayRule.Rules.Select(r => r.ToReadOnly()).ToReadOnlyCollection();
		}

		public ReadOnlyCollection<IReadOnlyRule> Rules { get; }

		IEnumerable<IRule> IArrayRule.Rules => Rules;
	}
}

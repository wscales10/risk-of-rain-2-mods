using MyRoR2;
using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Readonly;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Rules.RuleTypes.Mutable
{
	public class ArrayRule : Rule, IArrayRule
	{
		public ArrayRule(params Rule[] rules) : this((IEnumerable<Rule>)rules)
		{
		}

		public ArrayRule(IEnumerable<Rule> rules) => Rules = rules.ToList();

		public List<Rule> Rules { get; }

		public override IEnumerable<(string, Rule)> Children => Rules.SelectMany(r => r.Children);

		IEnumerable<IRule> IArrayRule.Rules => Rules;

		public override Bucket GetBucket(Context c)
		{
			foreach (var rule in Rules)
			{
				var bucket = rule.GetBucket(c);

				if (!(bucket is null))
				{
					return bucket;
				}
			}

			return null;
		}

		public override IReadOnlyRule ToReadOnly() => new ReadOnlyArrayRule(this);

		public override XElement ToXml()
		{
			var element = base.ToXml();
			element.Add(Rules.Select(b => b.ToXml()).ToArray());
			return element;
		}
	}
}
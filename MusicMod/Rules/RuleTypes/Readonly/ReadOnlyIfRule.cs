using MyRoR2;
using Patterns;
using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Mutable;

namespace Rules.RuleTypes.Readonly
{
	public class ReadOnlyIfRule : ReadOnlyRule<IfRule>, IIfRule
	{
		public ReadOnlyIfRule(IfRule ifRule) : base(ifRule)
		{
			Pattern = new ReadOnlyPattern<Context>(ifRule.Pattern);
			ThenRule = ifRule.ThenRule.ToReadOnly();
			ElseRule = ifRule.ElseRule?.ToReadOnly();
		}

		public IPattern<Context> Pattern { get; }

		public IRule ThenRule { get; }

		public IRule ElseRule { get; }
	}
}

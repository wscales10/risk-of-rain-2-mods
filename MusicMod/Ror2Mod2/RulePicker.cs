using Rules.RuleTypes.Interfaces;

namespace Ror2Mod2
{
	public abstract class RulePicker
	{
		public abstract IRule GetRule();
	}

	public class SingleRulePicker : RulePicker
	{
		private readonly IRule rule;

		public SingleRulePicker(IRule rule)
		{
			this.rule = rule;
		}

		public override IRule GetRule()
		{
			return rule;
		}
	}
}

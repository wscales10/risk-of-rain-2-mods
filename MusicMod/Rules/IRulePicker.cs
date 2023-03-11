using Rules.RuleTypes.Interfaces;

namespace Rules
{
	public interface IRulePicker<TContext, TOut>
	{
		IRule<TContext, TOut> Rule { get; }
	}

	public class SingleRulePicker<TContext, TOut> : IRulePicker<TContext, TOut>
	{
		public SingleRulePicker(IRule<TContext, TOut> rule) => Rule = rule;

		public IRule<TContext, TOut> Rule { get; }
	}

	public class MutableRulePicker<TContext, TOut> : IRulePicker<TContext, TOut>
	{
		public IRule<TContext, TOut> Rule { get; set; }
	}

	public class MutableRulePickerPicker<TContext, TOut> : IRulePicker<TContext, TOut>
	{
		public IRulePicker<TContext, TOut> RulePicker { get; set; }

		public IRule<TContext, TOut> Rule => RulePicker.Rule;
	}
}
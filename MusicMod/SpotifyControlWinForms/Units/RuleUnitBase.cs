using Rules;
using Utils;

namespace SpotifyControlWinForms.Units
{
	public abstract class RuleUnitBase<TIn, TOut> : Unit<TIn, TOut>, IRuleUnit
	{
		protected RuleUnitBase(string name, IRulePicker<TIn, TOut> rulePicker) : base(name)
		{
			RulePicker = rulePicker;
		}

		public virtual IRulePicker<TIn, TOut> RulePicker { get; }

		public abstract void SetRule(string? location);

		protected override TOut Transform(TIn input)
		{
			this.Log($"[{DateTime.Now}] {nameof(Transform)}");
			return RulePicker.Rule.GetOutput(input);
		}

		protected virtual void HandleRuleOutput(TOut ruleOutput, IList<string> changedPropertyNames) => Output(ruleOutput, changedPropertyNames);
	}
}
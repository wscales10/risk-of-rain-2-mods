using Rules;
using Rules.RuleTypes.Interfaces;
using Utils;

namespace SpotifyControlWinForms.Units
{
	internal abstract class RuleUnit<TIn, TOut> : RuleUnitBase<TIn, TOut>
	{
		private TOut? cachedOutput;

		protected RuleUnit(string name, RuleParser<TIn, TOut> ruleParser) : base(name, new MutableRulePicker<TIn, TOut>())
		{
			RuleParser = ruleParser;
		}

		public override MutableRulePicker<TIn, TOut> RulePicker => (MutableRulePicker<TIn, TOut>)base.RulePicker;

		public virtual IRule<TIn, TOut>? DefaultRule => null;

		public RuleParser<TIn, TOut> RuleParser { get; }

		public RuleUnit<TIn, TOut> Init(Func<UnitBase, string?> getRuleLocation)
		{
			SetRule(getRuleLocation(this));
			return this;
		}

		public override void SetRule(string? location)
		{
			SpotifyControl.SetRule(RulePicker, location, RuleParser, DefaultRule);
		}

		protected override void HandleRuleOutput(TOut ruleOutput, IList<string> changedPropertyNames)
		{
			this.Log($"[{DateTime.Now}] {Name}: {nameof(HandleRuleOutput)}");

			if (!Equals(cachedOutput, ruleOutput))
			{
				Output(cachedOutput = ruleOutput, changedPropertyNames);
			}
		}

		protected override void HandleInput(TIn input, IList<string> changedPropertyNames)
		{
			this.Log($"[{DateTime.Now}] {Name}: {nameof(HandleInput)}");
			var transformedInput = Transform(input);
			HandleRuleOutput(transformedInput, changedPropertyNames);
		}
	}
}
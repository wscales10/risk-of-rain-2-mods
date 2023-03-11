using Rules;
using Utils;

namespace SpotifyControlWinForms.Units
{
	public abstract class RuleUnitBase<TIn, TOut> : Unit<TIn, TOut>
	{
		protected RuleUnitBase(string name, IRulePicker<TIn, TOut> rulePicker) : base(name)
		{
			RulePicker = rulePicker;
		}

		public virtual IRulePicker<TIn, TOut> RulePicker { get; }

		protected override TOut Transform(TIn input)
		{
			this.Log($"[{DateTime.Now}] {nameof(Transform)}");
			return RulePicker.Rule.GetOutput(input);
		}
	}
}
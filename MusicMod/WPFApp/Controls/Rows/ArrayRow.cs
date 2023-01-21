using Rules.RuleTypes.Mutable;

namespace WPFApp.Controls.Rows
{
	internal class ArrayRow<TContext, TOut> : RuleRow<ArrayRow<TContext, TOut>, TContext, TOut>
	{
		internal ArrayRow(NavigationContext navigationContext) : base(navigationContext, true)
		{
		}

		public override string Label => Output?.ToString();

		protected override ArrayRow<TContext, TOut> deepClone() => new(NavigationContext);
	}
}
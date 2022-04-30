using Rules.RuleTypes.Mutable;

namespace WPFApp.Controls.Rows
{
	internal class ArrayRow : RuleRow<ArrayRow>
	{
		internal ArrayRow(Rule rule) : base(rule, true)
		{
		}

		public override string Label => Output.ToString();
	}
}
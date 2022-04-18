using Rules.RuleTypes.Mutable;

namespace WPFApp.Controls.Rows
{
	internal class ArrayRow : RuleRow<ArrayRow>
	{
		internal ArrayRow(Rule rule, NodeGetter<ArrayRow> nodeGetter) : base(rule, nodeGetter, true) { }
	}
}

using Rules.RuleTypes.Mutable;

namespace WPFApp.Controls.Rows
{
	internal class ArrayRow : RuleRow<ArrayRow>
	{
		internal ArrayRow(Rule rule, IndexGetter<ArrayRow> indexGetter) : base(rule, indexGetter, true)
		{
		}
	}
}
using Rules.RuleTypes.Mutable;
using System.ComponentModel;

namespace WPFApp.Controls.Rows
{
	public interface IRuleRow : IRow
	{
		string Label { get; }

		Rule Output { get; }

		ICollectionView Children { get; }

		IRuleRow Parent { get; set; }
	}
}
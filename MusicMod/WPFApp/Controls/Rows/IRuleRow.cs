using Rules.RuleTypes.Mutable;
using System.ComponentModel;
using WPFApp.ViewModels;

namespace WPFApp.Controls.Rows
{
	public interface IRuleRow : IRow
	{
		string Label { get; }

		NavigationViewModelBase OutputViewModel { get; }

		RuleBase Output { get; }

		ICollectionView Children { get; }

		IRuleRow Parent { get; set; }
	}
}
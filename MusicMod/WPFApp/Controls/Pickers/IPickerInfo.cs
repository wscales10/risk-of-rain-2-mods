using System.Collections;
using WPFApp.Controls.Wrappers;

namespace WPFApp.Controls.Pickers
{
	public interface IPickerInfo
	{
		string DisplayMemberPath { get; }

		string SelectedValuePath { get; }

		NavigationContext NavigationContext { get; }

		IEnumerable GetItems();

		IReadableControlWrapper CreateWrapper(object selectedInfo);
	}
}
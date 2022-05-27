using System.Collections;

namespace WPFApp.ViewModels.Pickers
{
    public interface IPickerInfo
    {
        string DisplayMemberPath { get; }

        string SelectedValuePath { get; }

        NavigationContext NavigationContext { get; }

        IEnumerable GetItems();

        object CreateItem(object selectedType);

        DiscreteMenuViewModel GetSelectorViewModel();
    }
}
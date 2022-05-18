using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace WPFApp.Controls.Pickers
{
    public abstract partial class Picker : UserControl
    {
        protected Picker() => DataContextChanged += Picker_DataContextChanged;

        protected abstract Selector ItemsControl { get; }

        public void CommitSelection()
        {
            if (ItemsControl.SelectedItem is not null)
            {
                GetViewModel().HandleSelection(GetViewModel().Config.CreateWrapper(ItemsControl.SelectedValue));
                ItemsControl.SelectedItem = null;
            }
        }

        protected PickerViewModel GetViewModel() => DataContext as PickerViewModel;

        protected void SetViewModel(PickerViewModel value) => DataContext = value;

        protected virtual void Picker_ViewModelChanged(PickerViewModel oldViewModel, PickerViewModel newViewModel)
        {
            ItemsControl.ItemsSource = newViewModel.ItemsSource;
            ItemsControl.DisplayMemberPath = newViewModel.Config.DisplayMemberPath;
            ItemsControl.SelectedValuePath = newViewModel.Config.SelectedValuePath;
            ItemsControl.SelectionChanged -= ItemsControl_SelectionChanged;
            ItemsControl.SelectionChanged += ItemsControl_SelectionChanged;
        }

        private void Picker_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) => Picker_ViewModelChanged(e.OldValue as PickerViewModel, (PickerViewModel)e.NewValue);

        private void ItemsControl_SelectionChanged(object sender, SelectionChangedEventArgs e) => CommitSelection();
    }
}
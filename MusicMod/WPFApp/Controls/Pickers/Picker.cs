using System.Windows;
using System.Windows.Controls;
using WPFApp.ViewModels.Pickers;

namespace WPFApp.Controls.Pickers
{
    public abstract partial class Picker : UserControl
    {
        protected Picker() => DataContextChanged += Picker_DataContextChanged;

        protected abstract Menu Selector { get; }

        public void CommitSelection(object value)
        {
            if (value is not null)
            {
                GetViewModel().HandleSelection(GetViewModel().Config.CreateWrapper(value));
            }
        }

        protected PickerViewModel GetViewModel() => DataContext as PickerViewModel;

        protected void SetViewModel(PickerViewModel value) => DataContext = value;

        protected virtual void Picker_ViewModelChanged(PickerViewModel oldViewModel, PickerViewModel newViewModel)
        {
            Selector.DataContext = newViewModel.SelectorViewModel;
            Selector.DisplayMemberPath = newViewModel.Config.DisplayMemberPath;
            Selector.SelectedValuePath = newViewModel.Config.SelectedValuePath;
            newViewModel.SelectorViewModel.OnObjectSelected -= CommitSelection;
            newViewModel.SelectorViewModel.OnObjectSelected += CommitSelection;
        }

        private void Picker_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) => Picker_ViewModelChanged(e.OldValue as PickerViewModel, (PickerViewModel)e.NewValue);
    }
}
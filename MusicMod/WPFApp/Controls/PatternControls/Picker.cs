using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace WPFApp.Controls.PatternControls
{
    public abstract partial class Picker : UserControl
    {
        protected Picker(PickerViewModel viewModel)
        {
            Init();
            ViewModel = viewModel;
        }

        public virtual PickerViewModel ViewModel
        {
            get => DataContext as PickerViewModel;

            private set
            {
                DataContext = value;
                ItemsControl.ItemsSource = value.ItemsSource;
                ItemsControl.DisplayMemberPath = value.Config.DisplayMemberPath;
                ItemsControl.SelectedValuePath = value.Config.SelectedValuePath;
                ItemsControl.SelectionChanged += ItemsControl_SelectionChanged;
            }
        }

        protected abstract Selector ItemsControl { get; }

        public void CommitSelection()
        {
            if (ItemsControl.SelectedItem is not null)
            {
                ViewModel.HandleSelection(ViewModel.Config.CreateWrapper(ItemsControl.SelectedValue));
                ItemsControl.SelectedItem = null;
            }
        }

        protected abstract void Init();

        private void ItemsControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CommitSelection();
        }
    }
}
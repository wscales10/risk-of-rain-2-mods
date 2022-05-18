using System.Windows;
using System.Windows.Controls.Primitives;

namespace WPFApp.Controls.Pickers
{
    /// <summary>
    /// Interaction logic for OptionalPicker.xaml
    /// </summary>
    public partial class OptionalPicker : Picker
    {
        public OptionalPicker() => InitializeComponent();

        public OptionalPickerViewModel ViewModel
        {
            get => (OptionalPickerViewModel)GetViewModel();
            set => SetViewModel(value);
        }

        protected override Selector ItemsControl => comboBox.ListBox;

        protected override void Picker_ViewModelChanged(PickerViewModel oldViewModel, PickerViewModel newViewModel)
        {
            if (oldViewModel is OptionalPickerViewModel oldOptionalPickerViewModel)
            {
                valueContainer.Deleted -= oldOptionalPickerViewModel.ClearValueWrapper;
            }

            base.Picker_ViewModelChanged(oldViewModel, newViewModel);

            if (newViewModel is OptionalPickerViewModel newOptionalPickerViewModel)
            {
                valueContainer.Deleted += newOptionalPickerViewModel.ClearValueWrapper;
            }
        }
    }
}
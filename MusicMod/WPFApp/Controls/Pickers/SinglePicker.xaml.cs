using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace WPFApp.Controls.Pickers
{
    /// <summary>
    /// Interaction logic for SinglePicker.xaml
    /// </summary>
    public partial class SinglePicker : Picker
    {
        public static readonly DependencyProperty AlignmentProperty = DependencyProperty.Register
        (
            nameof(Alignment),
            typeof(HorizontalAlignment),
            typeof(SinglePicker),
            new PropertyMetadata(HorizontalAlignment.Center)
        );

        public SinglePicker() => InitializeComponent();

        public SinglePickerViewModel ViewModel
        {
            get => (SinglePickerViewModel)GetViewModel();
            set => SetViewModel(value);
        }

        public HorizontalAlignment Alignment
        {
            get => (HorizontalAlignment)GetValue(AlignmentProperty);
            set => SetValue(AlignmentProperty, value);
        }

        public ComboBox comboBox => (ComboBox)aligner.Child;

        protected override Selector ItemsControl => comboBox;

        protected override void Picker_ViewModelChanged(PickerViewModel oldViewModel, PickerViewModel newViewModel)
        {
            if (oldViewModel is SinglePickerViewModel oldSinglePickerViewModel)
            {
                valueContainer.Deleted -= oldSinglePickerViewModel.ClearValueWrapper;
            }

            base.Picker_ViewModelChanged(oldViewModel, newViewModel);

            if (newViewModel is SinglePickerViewModel newSinglePickerViewModel)
            {
                valueContainer.Deleted += newSinglePickerViewModel.ClearValueWrapper;
            }
        }
    }
}
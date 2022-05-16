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

        public SinglePicker() : this(null)
        {
        }

        public SinglePicker(SinglePickerViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
            PostInit();
            valueContainer.Deleted += () => ViewModel.SetValueWrapper(null);
        }

        public HorizontalAlignment Alignment
        {
            get => (HorizontalAlignment)GetValue(AlignmentProperty);
            set => SetValue(AlignmentProperty, value);
        }

        public override SinglePickerViewModel ViewModel => (SinglePickerViewModel)base.ViewModel;

        public ComboBox comboBox => (ComboBox)aligner.Child;

        protected override Selector ItemsControl => comboBox;
    }
}
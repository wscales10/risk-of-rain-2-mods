using System.Windows.Controls.Primitives;

namespace WPFApp.Controls.PatternControls
{
    /// <summary>
    /// Interaction logic for OptionalPicker.xaml
    /// </summary>
    public partial class OptionalPicker : Picker
    {
        public OptionalPicker() : this(null)
        {
        }

        public OptionalPicker(OptionalPickerViewModel viewModel) : base(viewModel)
        {
            valueContainer.Deleted += () => ViewModel.ValueWrapper = null;
        }

        public override OptionalPickerViewModel ViewModel => (OptionalPickerViewModel)base.ViewModel;

        protected override Selector ItemsControl => comboBox.ListBox;

        protected override void Init() => InitializeComponent();
    }
}
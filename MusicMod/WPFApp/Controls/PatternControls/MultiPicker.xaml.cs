using System.Windows.Controls.Primitives;

namespace WPFApp.Controls.PatternControls
{
    /// <summary>
    /// Interaction logic for MultiPicker.xaml
    /// </summary>
    public partial class MultiPicker : Picker
    {
        public MultiPicker() : this(null)
        {
        }

        public MultiPicker(MultiPickerViewModel viewModel) : base(viewModel)
        {
        }

        public override MultiPickerViewModel ViewModel => (MultiPickerViewModel)base.ViewModel;

        protected override Selector ItemsControl => comboBox.ListBox;

        protected override void Init() => InitializeComponent();
    }
}
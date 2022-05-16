using System.Windows.Controls.Primitives;

namespace WPFApp.Controls.Pickers
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
            InitializeComponent();
            PostInit();
        }

        public override MultiPickerViewModel ViewModel => (MultiPickerViewModel)base.ViewModel;

        protected override Selector ItemsControl => comboBox.ListBox;
    }
}
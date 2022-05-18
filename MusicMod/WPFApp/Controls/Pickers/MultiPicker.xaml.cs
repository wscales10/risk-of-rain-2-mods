using System.Windows.Controls.Primitives;

namespace WPFApp.Controls.Pickers
{
    /// <summary>
    /// Interaction logic for MultiPicker.xaml
    /// </summary>
    public partial class MultiPicker : Picker
    {
        public MultiPicker()
        {
            InitializeComponent();
        }

        public MultiPickerViewModel ViewModel
        {
            get => (MultiPickerViewModel)GetViewModel();

            set => SetViewModel(value);
        }

        protected override Selector ItemsControl => comboBox.ListBox;
    }
}
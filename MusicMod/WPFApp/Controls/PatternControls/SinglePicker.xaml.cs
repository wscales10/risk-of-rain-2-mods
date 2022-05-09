using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace WPFApp.Controls.PatternControls
{
    /// <summary>
    /// Interaction logic for SinglePicker.xaml
    /// </summary>
    public partial class SinglePicker : Picker
    {
        public SinglePicker() : this(null)
        {
        }

        public SinglePicker(SinglePickerViewModel viewModel) : base(viewModel)
        {
            valueContainer.Deleted += () => ViewModel.SetValueWrapper(null);
        }

        public override SinglePickerViewModel ViewModel => (SinglePickerViewModel)base.ViewModel;

        protected override Selector ItemsControl => comboBox;

        protected override void Init() => InitializeComponent();
    }
}
using WPFApp.Controls.Wrappers;
using WPFApp.Controls.GridManagers;

namespace WPFApp.Controls.Pickers
{
    public class MultiPickerViewModel : PickerViewModel
    {
        public MultiPickerViewModel(IPickerInfo config) : base(config)
        {
        }

        public ValueContainerManager ValueContainerManager { get; } = new();

        internal ValueContainer AddWrapper(IReadableControlWrapper valueWrapper) => ValueContainerManager.Add(new(valueWrapper));

        protected override void handleSelection(IReadableControlWrapper valueWrapper) => _ = AddWrapper(valueWrapper);
    }
}
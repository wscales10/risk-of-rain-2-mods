using WPFApp.Controls.Wrappers;

namespace WPFApp.Controls.Pickers
{
    public class OptionalPickerViewModel : PickerViewModel
    {
        private IReadableControlWrapper valueWrapper;

        public OptionalPickerViewModel(IPickerInfo config) : base(config)
        {
        }

        public IReadableControlWrapper ValueWrapper
        {
            get => valueWrapper;
            set => SetProperty(ref valueWrapper, value);
        }

        protected override void handleSelection(IReadableControlWrapper valueWrapper) => ValueWrapper = valueWrapper;
    }
}
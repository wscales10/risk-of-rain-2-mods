namespace WPFApp.ViewModels.Pickers
{
    public class OptionalPickerViewModel : PickerViewModel
    {
        private IReadableControlWrapper valueWrapper;

        public OptionalPickerViewModel(IPickerInfo config) : base(config)
        {
        }

        public void ClearValueWrapper() => ValueWrapper = null;

        protected override void HandleSelection(IReadableControlWrapper valueWrapper) => ValueWrapper = valueWrapper;
    }
}
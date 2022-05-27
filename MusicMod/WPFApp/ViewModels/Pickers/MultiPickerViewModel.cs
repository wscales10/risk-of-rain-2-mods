using WPFApp.Controls.GridManagers;

namespace WPFApp.ViewModels.Pickers
{
    public class MultiPickerViewModel : PickerViewModel
    {
        public MultiPickerViewModel(IPickerInfo config) : base(config)
        {
        }

        public GridManager<IReadableControlWrapper> ValueWrapperManager { get; } = new();

        public DiscreteMenuViewModel<object> MenuViewModel { get; }

        internal IReadableControlWrapper AddWrapper(IReadableControlWrapper valueWrapper) => ValueWrapperManager.Add(valueWrapper);

        protected override void handleSelection(IReadableControlWrapper valueWrapper) => _ = AddWrapper(valueWrapper);
    }
}
using System.Collections;

namespace WPFApp.ViewModels.Pickers
{
    public abstract class PickerViewModel : ViewModelBase
    {
        private object selectedItem;

        private object selectedType;

        protected PickerViewModel(IPickerInfo config)
        {
            Config = config;
            ItemsSource = config.GetItems();
            SelectorViewModel = config.GetSelectorViewModel();
        }

        public IPickerInfo Config { get; }

        public IEnumerable ItemsSource { get; }

        public DiscreteMenuViewModel SelectorViewModel { get; }

        public abstract void HandleSelection(object item);
    }
}
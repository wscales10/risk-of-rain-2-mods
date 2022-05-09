using WPFApp.ViewModels;
using WPFApp.Controls.Wrappers;
using System.Collections;
using System.ComponentModel;

namespace WPFApp.Controls.PatternControls
{
    public abstract class PickerViewModel : ViewModelBase
    {
        private object selectedItem;

        private object selectedValue;

        protected PickerViewModel(IPickerInfo config)
        {
            Config = config;
            ItemsSource = config.GetItems();
        }

        public IPickerInfo Config { get; }

        public IEnumerable ItemsSource { get; }

        public object SelectedItem
        {
            get => selectedItem;
            set => SetProperty(ref selectedItem, value);
        }

        public object SelectedValue
        {
            get => selectedValue;
            set => SetProperty(ref selectedValue, value);
        }

        public void HandleSelection(IReadableControlWrapper valueWrapper)
        {
            handleSelection(valueWrapper);
            valueWrapper?.Focus();
        }

        protected abstract void handleSelection(IReadableControlWrapper valueWrapper);
    }
}
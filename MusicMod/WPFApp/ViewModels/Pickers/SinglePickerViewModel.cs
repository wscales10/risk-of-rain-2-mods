using System;

namespace WPFApp.ViewModels.Pickers
{
    public class SinglePickerViewModel : PickerViewModel
    {
        private object selectedItem;

        private object selectedType;

        public SinglePickerViewModel(IPickerInfo config) : base(config)
        {
        }

        public event Action ValueChanged;

        public object SelectedItem
        {
            get => selectedItem;
            set => SetProperty(ref selectedItem, value);
        }

        public object SelectedType
        {
            get => selectedType;
            set => SetProperty(ref selectedType, value);
        }

        public void ClearValueWrapper() => SetItem(null);

        public void SetItem(object item)
        {
            if (valueWrapper is not null)
            {
                valueWrapper.ValueSet += NotifyValueChanged;
            }

            if (ValueWrapper is not null)
            {
                ValueWrapper.ValueSet -= NotifyValueChanged;
            }

            SelectedItem = item;
            NotifyValueChanged();
        }

        protected override void HandleSelection(object item) => SetItem(item);

        private void NotifyValueChanged(object _ = null) => ValueChanged?.Invoke();
    }
}
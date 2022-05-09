using System;
using System.ComponentModel;
using WPFApp.Controls.Wrappers;

namespace WPFApp.Controls.PatternControls
{
    public class SinglePickerViewModel : PickerViewModel
    {
        private IReadableControlWrapper valueWrapper;

        public SinglePickerViewModel(IPickerInfo config) : base(config)
        {
        }

        public event Action ValueChanged;

        public IReadableControlWrapper ValueWrapper
        {
            get => valueWrapper;
            private set => SetProperty(ref valueWrapper, value);
        }

        public void SetValueWrapper(IReadableControlWrapper valueWrapper)
        {
            if (valueWrapper is not null)
            {
                valueWrapper.ValueSet += NotifyValueChanged;
            }

            if (ValueWrapper is not null)
            {
                ValueWrapper.ValueSet -= NotifyValueChanged;
            }

            ValueWrapper = valueWrapper;
            NotifyValueChanged();
        }

        protected override void handleSelection(IReadableControlWrapper valueWrapper) => SetValueWrapper(valueWrapper);

        private void NotifyValueChanged(object _ = null) => ValueChanged?.Invoke();
    }
}
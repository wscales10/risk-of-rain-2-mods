using System.ComponentModel;
using WPFApp.Controls.Pickers;
using WPFApp.Wrappers.SaveResults;
using WPFApp.ViewModels.Pickers;

namespace WPFApp.Wrappers
{
    internal abstract class SinglePickerWrapper<T> : ControlWrapper<T, SinglePicker>
    {
        protected SinglePickerWrapper(IPickerInfo config)
        {
            NavigationContext = config.NavigationContext;
            UIElement = new() { ViewModel = new(config) };
            UIElement.ViewModel.ValueChanged += NotifyValueChanged;
            UIElement.ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            SetPropertyDependency(nameof(ValueString), UIElement.ViewModel, nameof(UIElement.ViewModel.ValueWrapper));
            SetValue(null);
        }

        public sealed override SinglePicker UIElement { get; }

        public override string ValueString => UIElement.ViewModel.ValueWrapper?.ValueString;

        protected NavigationContext NavigationContext { get; }

        protected sealed override SaveResult<T> tryGetValue(GetValueRequest request)
        {
            var valueWrapper = UIElement.ViewModel.ValueWrapper;

            if (valueWrapper is null)
            {
                return new(null);
            }

            return SaveResult.Create<T>(valueWrapper.TryGetObject(request));
        }

        protected sealed override void setStatus(bool? status) => Outline(UIElement.comboBox, status is null ? null : UIElement.ViewModel.ValueWrapper is not null);

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SinglePickerViewModel.ValueWrapper):
                    var viewModel = (SinglePickerViewModel)sender;
                    RemovePropertyDependency(nameof(ValueString), nameof(IControlWrapper.ValueString));
                    SetPropertyDependency(nameof(ValueString), viewModel.ValueWrapper, nameof(IControlWrapper.ValueString));
                    break;
            }
        }
    }
}
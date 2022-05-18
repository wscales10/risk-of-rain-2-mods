using Patterns;
using WPFApp.Controls.Pickers;
using WPFApp.Controls.Wrappers.SaveResults;

namespace WPFApp.Controls.Wrappers.PatternWrappers
{
    public class OptionalPickerWrapper<T> : ControlWrapper<IPattern<T>, OptionalPicker>
    {
        private readonly NavigationContext navigationContext;

        public OptionalPickerWrapper(NavigationContext navigationContext)
        {
            var config = new PatternPickerInfo(typeof(T), navigationContext);
            UIElement = new() { ViewModel = new(config) };
            this.navigationContext = navigationContext;
        }

        public override OptionalPicker UIElement { get; }

        protected override void setValue(IPattern<T> value) => UIElement.ViewModel.HandleSelection(value is null ? null : PatternWrapper.Create(value, navigationContext));

        protected override SaveResult<IPattern<T>> tryGetValue(GetValueRequest request)
        {
            var patternWrapper = UIElement.ViewModel.ValueWrapper;

            if (patternWrapper is null)
            {
                return new((bool?)null);
            }

            var result = patternWrapper.TryGetObject(request);
            return new(result, result.IsSuccess ? (IPattern<T>)result.Value : default);
        }
    }
}
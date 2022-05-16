using Patterns;
using WPFApp.Controls.Pickers;

namespace WPFApp.Controls.Wrappers.PatternWrappers
{
    internal class SinglePatternPickerWrapper<T> : SinglePickerWrapper<IPattern<T>>
    {
        public SinglePatternPickerWrapper(NavigationContext navigationContext) : base(new PatternPickerInfo(typeof(T), navigationContext))
        {
        }

        protected override void setValue(IPattern<T> value) => UIElement.ViewModel.HandleSelection(value is null ? null : PatternWrapper.Create(value, NavigationContext));
    }
}
using Patterns.Patterns;
using WPFApp.Controls.PatternControls;
using Patterns;

namespace WPFApp.Controls.Wrappers.PatternWrappers
{
    internal class NotPatternWrapper<T> : ReadOnlyPatternWrapper<NotPattern<T>, NotPatternControl>
    {
        public NotPatternWrapper(NotPattern<T> pattern, NavigationContext navigationContext) : base(pattern)
        {
            UIElement = new(navigationContext, typeof(T));

            if (pattern is not null)
            {
                UIElement.PickerWrapper.SetValue(pattern.Child);
            }
        }

        public override NotPatternControl UIElement { get; }

        protected override SaveResult<NotPattern<T>> tryGetValue(bool trySave)
        {
            var result = UIElement.PickerWrapper.TryGetObject(trySave);

            if (result.IsSuccess)
            {
                Pattern.Child = (IPattern<T>)result.Value;
            }

            return new(result, Pattern);
        }
    }
}
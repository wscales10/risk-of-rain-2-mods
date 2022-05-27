using Patterns.Patterns;
using WPFApp.Controls.PatternControls;
using Patterns;
using WPFApp.Wrappers.SaveResults;

namespace WPFApp.Wrappers.PatternWrappers
{
    internal abstract class OnlyChildPatternWrapper<T, TPattern> : ReadOnlyPatternWrapper<TPattern, OnlyChildPatternControl>
        where TPattern : IOnlyChildPattern<T>
    {
        protected OnlyChildPatternWrapper(TPattern pattern, NavigationContext navigationContext) : base(pattern)
        {
            UIElement = new(navigationContext, typeof(T)) { ParentName = ParentName };

            if (pattern is not null)
            {
                UIElement.PickerWrapper.SetValue(pattern.Child);
            }
        }

        public override bool NeedsRightMargin => false;

        public override OnlyChildPatternControl UIElement { get; }

        protected abstract string ParentName { get; }

        protected override SaveResult<TPattern> tryGetValue(GetValueRequest request)
        {
            var result = UIElement.PickerWrapper.TryGetObject(request.TrySave);

            if (result.IsSuccess)
            {
                Pattern.Child = (IPattern<T>)result.Value;
            }

            return new(result, Pattern);
        }
    }
}
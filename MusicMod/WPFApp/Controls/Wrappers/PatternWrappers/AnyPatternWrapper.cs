using Patterns.Patterns.CollectionPatterns;

namespace WPFApp.Controls.Wrappers.PatternWrappers
{
    internal class AnyPatternWrapper<T> : OnlyChildPatternWrapper<T, AnyPattern<T>>
    {
        public AnyPatternWrapper(AnyPattern<T> pattern, NavigationContext navigationContext) : base(pattern, navigationContext)
        {
        }

        protected override string ParentName => "Any";
    }
}
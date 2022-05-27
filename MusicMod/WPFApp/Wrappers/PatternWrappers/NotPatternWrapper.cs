using Patterns.Patterns;

namespace WPFApp.Wrappers.PatternWrappers
{
    internal class NotPatternWrapper<T> : OnlyChildPatternWrapper<T, NotPattern<T>>
    {
        public NotPatternWrapper(NotPattern<T> pattern, NavigationContext navigationContext) : base(pattern, navigationContext)
        {
        }

        protected override string ParentName => "Not";
    }
}
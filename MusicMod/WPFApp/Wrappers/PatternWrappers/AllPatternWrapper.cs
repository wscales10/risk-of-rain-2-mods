using Patterns.Patterns.CollectionPatterns;

namespace WPFApp.Wrappers.PatternWrappers
{
    internal class AllPatternWrapper<T> : OnlyChildPatternWrapper<T, AllPattern<T>>
    {
        public AllPatternWrapper(AllPattern<T> pattern, NavigationContext navigationContext) : base(pattern, navigationContext)
        {
        }

        protected override string ParentName => "All";
    }
}
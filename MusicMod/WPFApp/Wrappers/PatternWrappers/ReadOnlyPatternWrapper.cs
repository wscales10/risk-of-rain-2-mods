using Patterns;
using System;
using System.Windows;
using WPFApp.Wrappers.SaveResults;

namespace WPFApp.Wrappers.PatternWrappers
{
    internal abstract class ReadOnlyPatternWrapper<TPattern, TControl> : PatternWrapper<TPattern, TControl>
        where TPattern : IPattern
        where TControl : FrameworkElement
    {
        protected ReadOnlyPatternWrapper(TPattern pattern) => Pattern = pattern;

        protected TPattern Pattern { get; }

        protected override void setValue(TPattern value) => throw new InvalidOperationException();

        protected override SaveResult<TPattern> tryGetValue(GetValueRequest request) => new(Pattern);
    }
}
using Patterns;
using System;
using System.Windows;
using WPFApp.Controls.Wrappers.SaveResults;

namespace WPFApp.Controls.Wrappers.PatternWrappers
{
    internal abstract class ReadOnlyPatternWrapper<TPattern, TControl> : PatternWrapper<TPattern, TControl>
		where TPattern : IPattern
		where TControl : FrameworkElement
	{
		protected ReadOnlyPatternWrapper(TPattern pattern) => Pattern = pattern;

		protected TPattern Pattern { get; }

		protected override void setValue(TPattern value) => throw new InvalidOperationException();

		protected override SaveResult<TPattern> tryGetValue(bool trySave) => new(Pattern);
	}
}
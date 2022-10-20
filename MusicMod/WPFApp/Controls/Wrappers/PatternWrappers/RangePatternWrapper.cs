using Patterns.Patterns.SmallPatterns;
using WPFApp.Controls.PatternControls;

namespace WPFApp.Controls.Wrappers.PatternWrappers
{
	internal abstract class RangePatternWrapper<TPattern> : PatternWrapper<TPattern, RangePatternControl>
		where TPattern : IValuePattern
	{
		public override RangePatternControl UIElement { get; } = new();
	}
}
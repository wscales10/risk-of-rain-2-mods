using Patterns.Patterns.SmallPatterns.ValuePatterns;
using WPFApp.Controls.Wrappers.SaveResults;
using Xceed.Wpf.Toolkit;

namespace WPFApp.Controls.Wrappers.PatternWrappers
{
	internal class IntPatternWrapper : RangePatternWrapper<IntPattern>
	{
		private readonly IntegerUpDown minUpDown;

		private readonly IntegerUpDown maxUpDown;

		public IntPatternWrapper(IntPattern pattern) : this()
		{
			setValue(pattern);
		}

		public IntPatternWrapper()
		{
			minUpDown = new();
			maxUpDown = new();
			UIElement.minContentPresenter.Content = minUpDown;
			UIElement.maxContentPresenter.Content = maxUpDown;
		}

		public override bool NeedsLeftMargin => false;

		protected override SaveResult<IntPattern> tryGetValue(GetValueRequest request)
		{
			return new(IntPattern.Create(minUpDown.Value, maxUpDown.Value));
		}

		protected override void setValue(IntPattern value)
		{
			minUpDown.Value = value?.Min;
			maxUpDown.Value = value?.Max;
		}

		protected override bool Validate(IntPattern value) => value.Min is not int min || value.Max is not int max || max >= min;
	}
}
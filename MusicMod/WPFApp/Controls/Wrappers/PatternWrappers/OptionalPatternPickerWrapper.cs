using Patterns;
using WPFApp.Controls.PatternControls;

namespace WPFApp.Controls.Wrappers.PatternWrappers
{
	internal class OptionalPatternPickerWrapper<T> : ControlWrapper<IPattern<T>, OptionalPatternPicker>
	{
		public OptionalPatternPickerWrapper(NavigationContext navigationContext) => UIElement = new(typeof(T), navigationContext);

		public override OptionalPatternPicker UIElement { get; }

		protected override void setValue(IPattern<T> value) => UIElement.AddPattern(value);

		protected override bool tryGetValue(out IPattern<T> value)
		{
			var patternWrapper = UIElement.patternContainer.PatternWrapper;

			if (patternWrapper is null)
			{
				value = default;
				return true;
			}

			if (patternWrapper.TryGetValue(out object output))
			{
				value = (IPattern<T>)output;
				return true;
			}

			value = default;
			return false;
		}
	}
}
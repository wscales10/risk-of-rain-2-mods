using Patterns;
using WPFApp.Controls.PatternControls;

namespace WPFApp.Controls.Wrappers.PatternWrappers
{
	internal class SinglePatternPickerWrapper<T> : ControlWrapper<IPattern<T>, SinglePatternPicker>
	{
		public SinglePatternPickerWrapper(NavigationContext navigationContext) => UIElement = new(typeof(T), navigationContext);

		public override SinglePatternPicker UIElement { get; }

		protected override void setValue(IPattern<T> value) => UIElement.AddPattern(value);

		protected override bool tryGetValue(out IPattern<T> value)
		{
			var patternWrapper = UIElement.patternContainer.PatternWrapper;

			if (patternWrapper is not null && patternWrapper.TryGetValue(out object output))
			{
				value = (IPattern<T>)output;
				return true;
			}

			value = default;
			return false;
		}

		protected override void SetStatus(bool status) => Outline(UIElement.patternContainer, UIElement.patternContainer.PatternWrapper is not null);
	}
}
using Patterns;
using WPFApp.Controls.PatternControls;

namespace WPFApp.Controls.Wrappers.PatternWrappers
{
	internal class SinglePatternPickerWrapper<T> : ControlWrapper<IPattern<T>, SinglePatternPicker>
	{
		public SinglePatternPickerWrapper(NavigationContext navigationContext)
		{
			UIElement = new(typeof(T), navigationContext);
			UIElement.ValueChanged += NotifyValueChanged;
			SetValue(null);
		}

		public override SinglePatternPicker UIElement { get; }

        protected override void setValue(IPattern<T> value) => UIElement.AddPattern(value);

		protected override SaveResult<IPattern<T>> tryGetValue(bool trySave)
		{
			var patternWrapper = UIElement.patternContainer.PatternWrapper;

			if (patternWrapper is null)
			{
				return new(false);
			}

			return SaveResult.Create<IPattern<T>>(patternWrapper.TryGetObject(trySave));
		}

		protected override void setStatus(bool? status) => Outline(UIElement.comboBox, status is null ? null : UIElement.patternContainer.PatternWrapper is not null);
	}
}
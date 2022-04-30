using Patterns;
using WPFApp.Controls.PatternControls;

namespace WPFApp.Controls.Wrappers.PatternWrappers
{
	internal class OptionalPatternPickerWrapper<T> : ControlWrapper<IPattern<T>, OptionalPatternPicker>
	{
		public OptionalPatternPickerWrapper(NavigationContext navigationContext) => UIElement = new(typeof(T), navigationContext);

		public override OptionalPatternPicker UIElement { get; }

		protected override void setValue(IPattern<T> value) => UIElement.AddPattern(value);

		protected override SaveResult<IPattern<T>> tryGetValue(bool trySave)
		{
			var patternWrapper = UIElement.patternContainer.PatternWrapper;

			if (patternWrapper is null)
			{
				return new((bool?)null);
			}

			var result = patternWrapper.TryGetObject(trySave);
			return new(result, result.IsSuccess ? (IPattern<T>)result.Value : default);
		}
	}
}
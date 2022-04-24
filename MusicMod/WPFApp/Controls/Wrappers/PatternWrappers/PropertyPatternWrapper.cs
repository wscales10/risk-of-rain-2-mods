using Patterns;
using Patterns.Patterns;
using WPFApp.Controls.PatternControls;
using Utils.Reflection;

namespace WPFApp.Controls.Wrappers.PatternWrappers
{
	internal class PropertyPatternWrapper<TObject> : PatternWrapper<PropertyPattern<TObject>, PropertyPatternControl>
	{
		protected PropertyPatternWrapper(PropertyPattern<TObject> pattern, NavigationContext navigationContext)
		{
			UIElement.NavigationContext = navigationContext;
			SetValue(pattern);
		}

		public override PropertyPatternControl UIElement { get; } = new(typeof(TObject));

		protected override void SetStatus(bool status) => Outline(UIElement.propertyComboBox, UIElement.SelectedProperty is not null);

		protected override void setValue(PropertyPattern<TObject> value)
		{
			UIElement.WaitingPattern = value?.Pattern;
			UIElement.SelectedProperty = value?.PropertyInfo;
		}

		protected override bool tryGetValue(out PropertyPattern<TObject> value)
		{
			var propertyInfo = UIElement.SelectedProperty;

			if (propertyInfo is null)
			{
				value = null;
				return false;
			}

			if (!UIElement.TryGetPattern(out IPattern pattern))
			{
				value = null;
				return false;
			}

			value = typeof(PropertyPattern<TObject>)
				.GetMethod("Create", new[] { typeof(string), typeof(IPattern) })
				.MakeGenericMethod(propertyInfo.Type)
				.InvokeStatic<PropertyPattern<TObject>>(propertyInfo.Name, pattern);
			return true;
		}
	}
}
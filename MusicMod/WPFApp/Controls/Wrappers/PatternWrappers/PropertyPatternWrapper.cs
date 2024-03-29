﻿using Patterns;
using Patterns.Patterns;
using WPFApp.Controls.PatternControls;
using Utils.Reflection;
using WPFApp.Controls.Wrappers.SaveResults;

namespace WPFApp.Controls.Wrappers.PatternWrappers
{
	internal class PropertyPatternWrapper<TObject> : PatternWrapper<PropertyPattern<TObject>, PropertyPatternControl>
	{
		protected PropertyPatternWrapper(PropertyPattern<TObject> pattern, NavigationContext navigationContext)
		{
			UIElement.NavigationContext = navigationContext;
			SetValue(pattern);
			UIElement.ValueChanged += NotifyValueChanged;
		}

		public override PropertyPatternControl UIElement { get; } = new(typeof(TObject));

		public override bool NeedsLeftMargin => false;

		protected override void setStatus(bool? status) => Outline(UIElement.propertyComboBox, status != false || UIElement.SelectedProperty is not null);

		protected override void setValue(PropertyPattern<TObject> value)
		{
			UIElement.WaitingPattern = value?.Pattern;
			UIElement.SelectedProperty = value?.PropertyInfo;
		}

		protected override SaveResult<PropertyPattern<TObject>> tryGetValue(GetValueRequest request)
		{
			var propertyInfo = UIElement.SelectedProperty;

			if (propertyInfo is null)
			{
				return new(false);
			}

			var result = UIElement.TryGetPattern(request);

			PropertyPattern<TObject> value;

			if (result.IsSuccess)
			{
				var type = typeof(Query<TObject>);
				var method1 = type.GetMethod("Create", new[] { typeof(string), typeof(IPattern) });
				var method2 = method1.MakeGenericMethod(propertyInfo.Type);
				value = method2.InvokeStatic<PropertyPattern<TObject>>(propertyInfo.Name, result.Value);
			}
			else
			{
				value = null;
			}

			return new(result, value);
		}
	}
}
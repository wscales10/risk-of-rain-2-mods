using Patterns;
using Patterns.Patterns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Utils;
using System.Windows;
using System.Windows.Media;

namespace WPFApp.Controls.Wrappers.PatternWrappers
{
	internal class PropertyPatternWrapper<TObject> : PatternWrapper<PropertyPattern<TObject>, StackPanel>
	{
		private readonly ComboBox propertyComboBox = new() { Width = 160 };

		private readonly Border patternPickerContainer = new();

		private readonly Dictionary<string, IControlWrapper> patternPickerWrappers = new();

		private readonly SetProperty<IControlWrapper> PatternPickerWrapper = new();

		private readonly NavigationContext navigationContext;

		private IPattern waitingPattern;

		protected PropertyPatternWrapper(PropertyPattern<TObject> pattern, NavigationContext navigationContext)
		{
			this.navigationContext = navigationContext;
			PatternPickerWrapper.OnSet += (_, value) => patternPickerContainer.Child = value?.UIElement;
			UIElement.Children.Add(propertyComboBox);
			UIElement.Children.Add(patternPickerContainer);

			propertyComboBox.ItemsSource = Info.GetProperties<TObject>();

			propertyComboBox.SelectionChanged += PropertyComboBox_SelectionChanged;
			SetValue(pattern);
		}

		public override StackPanel UIElement { get; } = new();

		protected override void setValue(PropertyPattern<TObject> value)
		{
			waitingPattern = value?.Pattern;
			if (((PropertyInfo)propertyComboBox.SelectedItem) == value?.PropertyInfo)
			{
				PropertyComboBox_SelectionChanged(this, null);
			}
			else
			{
				propertyComboBox.SelectedItem = value?.PropertyInfo;
			}
		}

		protected override bool tryGetValue(out PropertyPattern<TObject> value)
		{
			var propertyInfo = (PropertyInfo)propertyComboBox.SelectedItem;

			if (propertyInfo is null)
			{
				value = null;
				return false;
			}

			if (!PatternPickerWrapper.Get().TryGetValue(out object pattern))
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

		private void PropertyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var selectedItem = (PropertyInfo)propertyComboBox.SelectedItem;

			if (selectedItem is null)
			{
				PatternPickerWrapper.Set(null);
			}
			else if (patternPickerWrappers.TryGetValue(selectedItem.Name, out var value))
			{
				PatternPickerWrapper.Set(value);
			}
			else
			{
				PatternPickerWrapper.Set((IControlWrapper)typeof(SinglePatternPickerWrapper<>).MakeGenericType(selectedItem.Type).Construct(navigationContext));
				patternPickerWrappers[selectedItem.Name] = PatternPickerWrapper.Get();
			}

			if (waitingPattern is not null)
			{
				PatternPickerWrapper.Get().SetValue(waitingPattern);
				waitingPattern = null;
			}
		}
	}
}
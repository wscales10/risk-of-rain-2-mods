using Patterns;
using Patterns.Patterns;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using WPFApp.Controls.Wrappers;
using WPFApp.Controls.Wrappers.PatternWrappers;
using System.Windows;
using Utils.Properties;
using Utils.Reflection;

namespace WPFApp.Controls.PatternControls
{
	/// <summary>
	/// Interaction logic for PropertyPatternControl.xaml
	/// </summary>
	public partial class PropertyPatternControl : UserControl
	{
		private readonly Dictionary<string, IControlWrapper> patternPickerWrappers = new();

		private readonly SetProperty<IControlWrapper> PatternPickerWrapper = new();

		private Type objectType;

		public PropertyPatternControl()
		{
			InitializeComponent();
			PatternPickerWrapper.OnSet += propertySetEventHandler;
		}

		public PropertyPatternControl(NavigationContext navigationContext) : this() => NavigationContext = navigationContext;

		public PropertyPatternControl(Type objectType, NavigationContext navigationContext) : this(navigationContext) => ObjectType = objectType;

		public PropertyPatternControl(Type objectType) : this() => ObjectType = objectType;

		public IPattern WaitingPattern { private get; set; }

		public NavigationContext NavigationContext { get; set; }

		public Type ObjectType
		{
			get => objectType;

			set
			{
				propertyComboBox.ItemsSource = Info.GetProperties(value);
				objectType = value;
			}
		}

		public PropertyInfo SelectedProperty
		{
			get => (PropertyInfo)propertyComboBox.SelectedItem;

			set
			{
				if ((PropertyInfo)propertyComboBox.SelectedItem == value)
				{
					propertyComboBox_SelectionChanged(this, null);
				}
				else
				{
					propertyComboBox.SelectedValue = value?.Name;
				}
			}
		}

		public bool TryGetPattern(out IPattern pattern)
		{
			if (PatternPickerWrapper.Get().TryGetValue(out object value))
			{
				pattern = (IPattern)value;
				return true;
			}

			pattern = default;
			return false;
		}

		private void propertySetEventHandler(IControlWrapper _, IControlWrapper value)
		{
			patternPickerContainer.Child = value?.UIElement;
			patternPickerLabel.Visibility = value is null ? Visibility.Hidden : Visibility.Visible;
		}

		private void propertyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var selectedItem = (PropertyInfo)propertyComboBox.SelectedItem;

			if (selectedItem is null)
			{
				PatternPickerWrapper.Set(null);
			}
			else if (patternPickerWrappers.TryGetValue(selectedItem.Name, out IControlWrapper value))
			{
				PatternPickerWrapper.Set(value);
			}
			else
			{
				PatternPickerWrapper.Set((IControlWrapper)typeof(SinglePatternPickerWrapper<>).MakeGenericType(selectedItem.Type).Construct(NavigationContext));
				patternPickerWrappers[selectedItem.Name] = PatternPickerWrapper.Get();
			}

			if (WaitingPattern is not null)
			{
				PatternPickerWrapper.Get().SetValue(WaitingPattern);
				WaitingPattern = null;
			}
		}
	}
}
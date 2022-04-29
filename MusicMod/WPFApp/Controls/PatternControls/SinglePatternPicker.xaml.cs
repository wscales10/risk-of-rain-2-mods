﻿using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using WPFApp.Controls.Wrappers;

namespace WPFApp.Controls.PatternControls
{
	/// <summary>
	/// Interaction logic for SinglePatternPicker.xaml
	/// </summary>
	public partial class SinglePatternPicker : PatternPicker
	{
		public SinglePatternPicker(Type valueType, NavigationContext navigationContext) : base(valueType, navigationContext) => patternContainer.Deleted += () => SetPatternWrapper(null);

		public event Action ValueChanged;

		protected override Selector ItemsControl => comboBox;

		protected override void handleSelection(IReadableControlWrapper patternWrapper) => SetPatternWrapper(patternWrapper);

		protected override void Init() => InitializeComponent();

		private void SetPatternWrapper(IReadableControlWrapper value)
		{
			if (value is null)
			{
				patternContainer.Visibility = Visibility.Collapsed;
				comboBox.Visibility = Visibility.Visible;
			}
			else
			{
				comboBox.Visibility = Visibility.Collapsed;
				patternContainer.Visibility = Visibility.Visible;
				value.ValueSet += NotifyValueChanged;
			}

			if (patternContainer.PatternWrapper is not null)
			{
				patternContainer.PatternWrapper.ValueSet -= NotifyValueChanged;
			}

			patternContainer.PatternWrapper = value;
			NotifyValueChanged();
		}

		private void NotifyValueChanged(object _ = null) => ValueChanged?.Invoke();
	}
}
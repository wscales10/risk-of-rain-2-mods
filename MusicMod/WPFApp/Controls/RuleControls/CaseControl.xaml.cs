using Patterns;
using Rules.RuleTypes.Mutable;
using System;
using System.Windows.Controls;
using WPFApp.Controls.PatternControls;
using WPFApp.Controls.Wrappers.PatternWrappers;
using System.Windows;
using MyRoR2;
using System.Windows.Media;
using WPFApp.Controls.Wrappers;

namespace WPFApp.Controls.RuleControls
{
	/// <summary>
	/// Interaction logic for CaseControl.xaml
	/// </summary>
	public partial class CaseControl : UserControl
	{
		private readonly NavigationContext navigationContext;

		private readonly MultiPatternPicker patternPicker;

		private readonly Func<SaveResult> tryGetValues;

		public CaseControl(Case<IPattern> c, Type valueType, NavigationContext navigationContext)
		{
			InitializeComponent();
			whereControl.NavigationContext = navigationContext;
			whereControl.PatternWrapper.StatusSet += WherePatternWrapper_StatusSet;
			whereControl.PatternWrapper.SetValue(c.WherePattern);
			this.navigationContext = navigationContext;
			patternPicker = new(valueType, navigationContext);
			patternPicker.VerticalAlignment = VerticalAlignment.Center;
			patternPickerContainer.Child = patternPicker;
			tryGetValues = patternPicker.PatternContainerManager.BindLooselyTo(c.Arr, AddPattern, (PatternContainer c) => SaveResult.Create<IPattern>(c.PatternWrapper.TryGetObject(true)));
			Case = c;
		}

		public Case<IPattern> Case { get; }

		public SaveResult TrySaveChanges()
		{
			SaveResult success = tryGetValues();
			var result = whereControl.PatternWrapper.TryGetObject(true);

			if (result.IsSuccess)
			{
				Case.WherePattern = (IPattern<Context>)result.Value;
			}

			return success & result;
		}

		private void WherePatternWrapper_StatusSet(bool? status)
		{
			ToggleButton.Background = status switch
			{
				false => Brushes.Red,
				_ => Brushes.White
			};
		}

		private PatternContainer AddPattern(IPattern pattern) => patternPicker.AddPatternWrapper(PatternWrapper.Create(pattern, navigationContext));

		private void ToggleButton_Checked(object sender, RoutedEventArgs e)
		{
			ButtonColumn.Width = new GridLength(ButtonColumn.ActualWidth);
			buttonContainer.Child = null;
			Grid.SetColumn((UIElement)sender, 1);
			whereControl.grid.Children.Add((UIElement)sender);
		}

		private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
		{
			whereControl.grid.Children.Remove((UIElement)sender);
			buttonContainer.Child = (UIElement)sender;
			ButtonColumn.Width = new GridLength(1, GridUnitType.Auto);
		}
	}
}
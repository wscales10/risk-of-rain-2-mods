using System;
using System.Windows.Controls.Primitives;
using WPFApp.Controls.Wrappers;
using System.Windows;

namespace WPFApp.Controls.PatternControls
{
	/// <summary>
	/// Interaction logic for OptionalPatternPicker.xaml
	/// </summary>
	public partial class OptionalPatternPicker : PatternPicker
	{
		public OptionalPatternPicker(Type valueType, NavigationContext navigationContext) : base(valueType, navigationContext)
		{
			patternContainer.Deleted += () => SetPatternWrapper(null);
		}

		protected override Selector ItemsControl => comboBox.ListBox;

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
			}

			patternContainer.PatternWrapper = value;
		}
	}
}
using System;
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
		public SinglePatternPicker(Type valueType, NavigationContext navigationContext) : base(valueType, navigationContext)
		{
		}

		protected override Selector ItemsControl => comboBox;

		protected override void Init() => InitializeComponent();

		protected override void HandleSelection(IReadableControlWrapper patternWrapper)
		{
			comboBox.Visibility = Visibility.Collapsed;
			patternContainer.Visibility = Visibility.Visible;
			patternContainer.PatternWrapper = patternWrapper;
			patternContainer.Deleted += () =>
			{
				patternContainer.PatternWrapper = null;
				comboBox.SelectedItem = null;
				patternContainer.Visibility = Visibility.Collapsed;
				comboBox.Visibility = Visibility.Visible;
			};
		}
	}
}
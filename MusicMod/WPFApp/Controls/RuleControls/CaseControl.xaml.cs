using System.Windows;
using System.Windows.Controls;

namespace WPFApp.Controls.RuleControls
{
	/// <summary>
	/// Interaction logic for CaseControl.xaml
	/// </summary>
	public partial class CaseControl : UserControl
	{
		public CaseControl() => InitializeComponent();

		private void ToggleButton_Checked(object sender, RoutedEventArgs e)
		{
			buttonContainer.Child = null;
			whereControl.buttonContainer.Child = (UIElement)sender;
		}

		private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
		{
			whereControl.buttonContainer.Child = null;
			buttonContainer.Child = (UIElement)sender;
		}
	}
}
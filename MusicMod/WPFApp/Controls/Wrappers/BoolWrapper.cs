using System.Windows.Controls;
using System.Windows;

namespace WPFApp.Controls.Wrappers
{
	internal class BoolWrapper : ControlWrapper<bool, CheckBox>
	{
		public override CheckBox UIElement { get; } = new CheckBox { VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };

		protected override void setValue(bool value) => UIElement.IsChecked = value;

		protected override bool tryGetValue(out bool value)
		{
			value = (bool)UIElement.IsChecked;
			return true;
		}
	}
}
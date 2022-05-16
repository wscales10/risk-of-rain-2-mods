using System.Windows.Controls;
using System.Windows;
using WPFApp.Controls.Wrappers.SaveResults;

namespace WPFApp.Controls.Wrappers
{
    internal class BoolWrapper : ControlWrapper<bool, CheckBox>
	{
		public BoolWrapper()
		{
			UIElement.Checked += (s, e) => NotifyValueChanged();
			UIElement.Unchecked += (s, e) => NotifyValueChanged();
		}

		public override CheckBox UIElement { get; } = new CheckBox { VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };

		public override string ValueString => UIElement?.IsChecked?.ToString() ?? "null";

		protected override void setValue(bool value) => UIElement.IsChecked = value;

		protected override SaveResult<bool> tryGetValue(bool trySave) => new(true, (bool)UIElement.IsChecked);
	}
}
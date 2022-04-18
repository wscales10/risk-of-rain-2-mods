using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace WPFApp.Controls.Wrappers
{
	internal class IntWrapper : ControlWrapper<int, IntegerUpDown>
	{
		public IntWrapper(int? min = null, int? max = null)
		{
			UIElement.Minimum = min;
			UIElement.Maximum = max;
		}

		public override IntegerUpDown UIElement { get; } = new();

		protected override void setValue(int value) => UIElement.Value = value;

		protected override bool tryGetValue(out int value)
		{
			int? input = UIElement.Value;
			value = input ?? default;
			return input is not null;
		}

		protected override bool Validate(int value)
			=> (UIElement.Minimum is null || value >= UIElement.Minimum)
			&& (UIElement.Maximum is null || value <= UIElement.Maximum);

		protected override void SetStatus(bool status) => UIElement.BorderBrush = status ? Brushes.DarkGray : Brushes.Red;
	}
}
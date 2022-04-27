using Xceed.Wpf.Toolkit;

namespace WPFApp.Controls.Wrappers
{
	internal class IntWrapper : ControlWrapper<int, IntegerUpDown>
	{
		public IntWrapper(int? min = null, int? max = null)
		{
			UIElement.Minimum = min;
			UIElement.Maximum = max;
			UIElement.ValueChanged += (s, e) => NotifyValueChanged();
		}

		public override IntegerUpDown UIElement { get; } = new();

		protected override void setValue(int value) => UIElement.Value = value;

		protected override SaveResult<int> tryGetValue(bool trySave)
		{
			int? input = UIElement.Value;
			return new(input is not null, input ?? 0);
		}

		protected override bool Validate(int value)
			=> (UIElement.Minimum is null || value >= UIElement.Minimum)
			&& (UIElement.Maximum is null || value <= UIElement.Maximum);
	}
}
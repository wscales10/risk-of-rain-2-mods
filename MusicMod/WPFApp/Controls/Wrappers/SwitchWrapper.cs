using Patterns;
using System.Windows.Media;
using System.Windows;

namespace WPFApp.Controls.Wrappers
{
	public delegate bool Parser<T>(string s, out T value);

	internal class SwitchWrapper<T, TOut> : ControlWrapper<Switch<T, TOut>, SwitchControl>
	{
		private readonly Parser<TOut> parser;

		public SwitchWrapper(Parser<TOut> parser) => this.parser = parser;

		public override SwitchControl UIElement { get; } = new() { BorderThickness = new Thickness(1) };

		protected override void setValue(Switch<T, TOut> value) => UIElement.SetValue(value, p => p.ToString(), o => o.ToString());

		protected override bool tryGetValue(out Switch<T, TOut> value) => UIElement.GetValue(parser, out value);

		protected override void SetStatus(bool status) => UIElement.BorderBrush = status ? Brushes.DarkGray : Brushes.Red;
	}
}
using Patterns;
using System.Windows;
using System.Windows.Controls;

namespace WPFApp.Controls.Wrappers.PatternWrappers
{
	internal class TestPatternWrapper : ReadOnlyPatternWrapper<IPattern, TextBlock>
	{
		public TestPatternWrapper(IPattern pattern) : base(pattern) => UIElement.Text = pattern?.ToString();

		public override TextBlock UIElement { get; } = new TextBlock
		{
			TextAlignment = TextAlignment.Center,
			FontSize = 14,
			Margin = new Thickness(4)
		};

		protected override void setStatus(bool status)
		{ }
	}
}
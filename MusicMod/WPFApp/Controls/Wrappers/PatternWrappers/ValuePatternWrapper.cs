using Patterns.Patterns.SmallPatterns;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WPFApp.Controls.Wrappers.PatternWrappers
{
	internal abstract class ValuePatternWrapper<TPattern, TControl> : ReadOnlyPatternWrapper<TPattern, TControl>
		where TPattern : IValuePattern
		where TControl : FrameworkElement
	{
		protected ValuePatternWrapper(TPattern pattern) : base(pattern)
		{
			TextBox.Text = GetTextBoxText();
			Display();
		}

		protected TextBox TextBox { get; } = new()
		{
			TextAlignment = TextAlignment.Center,
			FontSize = 14,
			Margin = new Thickness(4, 2, 4, 2)
		};

		protected override bool Validate(TPattern value) => value.Definition is not null;

		protected virtual void DefineWith(string textBoxText) => _ = Pattern.DefineWith(textBoxText);

		protected virtual string GetTextBoxText() => Pattern.Definition;

		protected override bool tryGetValue(out TPattern value)
		{
			Display();
			return base.tryGetValue(out value);
		}

		protected virtual void Display()
		{
			DefineWith(TextBox.Text);

			if (Validate(Pattern))
			{
				TextBox.IsReadOnly = true;
				TextBox.BorderThickness = new Thickness(0);
			}
			else
			{
				TextBox.IsReadOnly = false;
				TextBox.BorderThickness = new Thickness(1);
			}
		}

		protected override void SetStatus(bool status) => TextBox.BorderBrush = status ? Brushes.DarkGray : Brushes.Red;
	}

	internal class ValuePatternWrapper<TPattern> : ValuePatternWrapper<TPattern, TextBox>
		where TPattern : IValuePattern
	{
		public ValuePatternWrapper(TPattern pattern) : base(pattern)
		{
		}

		public override TextBox UIElement => TextBox;
	}
}
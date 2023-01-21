using Patterns.Patterns.SmallPatterns;
using System.Windows;
using System.Windows.Controls;
using WPFApp.Controls.Wrappers.SaveResults;

namespace WPFApp.Controls.Wrappers.PatternWrappers
{
	internal abstract class ValuePatternWrapper<TPattern, TControl> : ReadOnlyPatternWrapper<TPattern, TControl>
		where TPattern : IValuePattern
		where TControl : FrameworkElement
	{
		protected ValuePatternWrapper(TPattern pattern) : base(pattern)
		{
			Text = GetTextBoxText();
			TextBox.TextAlignment = TextAlignment.Center;
			TextBox.VerticalAlignment = VerticalAlignment.Center;
			TextBox.FontSize = 14;
			TextBox.Margin = new Thickness(2);
			TextBox.LostFocus += (s, e) => NotifyValueChanged();
			Display();
		}

		public override UIElement FocusElement => TextBox;

		protected virtual TextBox TextBox { get; } = new();

		protected virtual string Text
		{
			get => TextBox.Text;
			set => TextBox.Text = value;
		}

		protected override bool Validate(TPattern value) => value.Definition is not null;

		protected virtual void DefineWith(string textBoxText) => _ = Pattern.DefineWith(textBoxText);

		protected virtual string GetTextBoxText() => Pattern.Definition;

		protected override SaveResult<TPattern> tryGetValue(GetValueRequest request)
		{
			Display();
			return base.tryGetValue(request);
		}

		protected virtual void Display()
		{
			DefineWith(Text);

			if (Validate(Pattern))
			{
				// TextBox.IsReadOnly = true;
				TextBox.BorderThickness = new Thickness(0);
			}
			else
			{
				// TextBox.IsReadOnly = false;
				TextBox.BorderThickness = new Thickness(1);
			}
		}

		protected override void setStatus(bool status) => Outline(TextBox, status);
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
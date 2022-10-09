using Patterns.Patterns;
using System.Windows;
using System.Windows.Controls;
using WPFApp.Controls.Wrappers.SaveResults;

namespace WPFApp.Controls.Wrappers.PatternWrappers
{
	internal class MathsPatternWrapper<TValue> : ReadOnlyPatternWrapper<MathsPattern<TValue>, TextBox>
		where TValue : struct
	{
		protected MathsPatternWrapper(MathsPattern<TValue> pattern) : base(pattern)
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

		public override TextBox UIElement => TextBox;

		protected virtual TextBox TextBox { get; } = new();

		protected virtual string Text
		{
			get => TextBox.Text;
			set => TextBox.Text = value;
		}

		protected override bool Validate(MathsPattern<TValue> value) => !string.IsNullOrWhiteSpace(value.ExpressionString);

		protected virtual void DefineWith(string textBoxText) => _ = Pattern.ExpressionString = textBoxText;

		protected virtual string GetTextBoxText() => Pattern.ExpressionString;

		protected override SaveResult<MathsPattern<TValue>> tryGetValue(GetValueRequest request)
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
}
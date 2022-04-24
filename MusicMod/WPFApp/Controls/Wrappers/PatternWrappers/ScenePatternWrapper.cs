using MyRoR2;
using System.Windows.Controls;
using WPFApp.Controls.PatternControls;

namespace WPFApp.Controls.Wrappers.PatternWrappers
{
	internal class ScenePatternWrapper : ValuePatternWrapper<ScenePattern, ScenePatternControl>
	{
		public ScenePatternWrapper(ScenePattern pattern) : base(pattern)
		{
		}

		public override ScenePatternControl UIElement { get; } = new();

		protected override TextBox TextBox => UIElement.textBox;

		protected override string GetTextBoxText() => Pattern.DisplayName;

		protected override void DefineWith(string textBoxText) => Pattern.DefineWithDisplayName(textBoxText);

		protected override bool Validate(ScenePattern value) => value.Definition is not null;

		protected override void Display()
		{
			base.Display();
			_ = UIElement.SetImageSourceAsync(TextBox.Text);
		}
	}
}
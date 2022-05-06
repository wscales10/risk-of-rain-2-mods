using MyRoR2;
using System.Windows.Controls;
using WPFApp.Controls.PatternControls;
using WPFApp.ViewModels;

namespace WPFApp.Controls.Wrappers.PatternWrappers
{
	internal class ScenePatternWrapper : ValuePatternWrapper<ScenePattern, ImagePatternControl>
	{
		private readonly ScenePatternViewModel viewModel = new();

		public ScenePatternWrapper(ScenePattern pattern) : base(pattern)
		{
		}

		public override ImagePatternControl UIElement { get; } = new();

		protected override TextBox TextBox => UIElement.textBox;

		protected override void Init()
		{
			base.Init();
			UIElement.DataContext = viewModel;
		}

		protected override string GetTextBoxText() => Pattern.DisplayName;

		protected override void DefineWith(string textBoxText) => Pattern.DefineWithDisplayName(textBoxText);

		protected override bool Validate(ScenePattern value) => value.Definition is not null;

		protected override void Display()
		{
			base.Display();
			viewModel.Text = TextBox.Text;
		}
	}
}
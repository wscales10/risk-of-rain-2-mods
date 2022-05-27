using MyRoR2;
using WPFApp.ViewModels;

namespace WPFApp.Wrappers.PatternWrappers
{
    internal class ScenePatternWrapper : ImagePatternWrapper<ScenePattern>
    {
        public ScenePatternWrapper(ScenePattern pattern) : base(pattern)
        {
        }

        protected override ScenePatternViewModel ViewModel { get; } = new();

        protected override string GetTextBoxText() => Pattern.DisplayName;

        protected override void DefineWith(string textBoxText) => Pattern.DefineWithDisplayName(textBoxText);

        protected override bool Validate(ScenePattern value) => value.Definition is not null;
    }
}
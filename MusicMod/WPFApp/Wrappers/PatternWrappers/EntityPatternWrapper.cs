using MyRoR2;
using WPFApp.ViewModels;

namespace WPFApp.Wrappers.PatternWrappers
{
    internal class EntityPatternWrapper : ImagePatternWrapper<EntityPattern>
    {
        public EntityPatternWrapper(EntityPattern pattern) : base(pattern)
        {
        }

        protected override EntityPatternViewModel ViewModel { get; } = new();

        protected override string GetTextBoxText() => Pattern.DisplayName;

        protected override void DefineWith(string textBoxText) => Pattern.DefineWithDisplayName(textBoxText);

        protected override bool Validate(EntityPattern value) => value.Definition is not null;
    }
}
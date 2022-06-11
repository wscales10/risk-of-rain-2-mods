using Rules.RuleTypes.Mutable;

namespace WPFApp.Controls.Rows
{
    internal class ArrayRow : RuleRow<ArrayRow>
    {
        internal ArrayRow(NavigationContext navigationContext) : base(navigationContext, true)
        {
        }

        public override string Label => Output?.ToString();

        protected override ArrayRow deepClone() => new(NavigationContext);
    }
}
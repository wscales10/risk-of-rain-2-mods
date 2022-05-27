namespace WPFApp.Rows
{
    internal class ArrayRow : RuleRow<ArrayRow>
    {
        internal ArrayRow(NavigationContext navigationContext) : base(navigationContext, true)
        {
        }

        public override string Label => Output?.ToString();
    }
}
using Rules.RuleTypes.Mutable;

namespace WPFApp.Controls.RuleControls
{
	public abstract class RuleControlBase : XmlControlBase<Rule>
	{
		protected RuleControlBase(NavigationContext navigationContext) : base(navigationContext)
		{
		}
	}
}
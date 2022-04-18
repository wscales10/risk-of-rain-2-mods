using Patterns;

namespace WPFApp.Controls.PatternControls
{
	public abstract class PatternControlBase : XmlControlBase<IPattern>
	{
		protected PatternControlBase(NavigationContext navigationContext) : base(navigationContext)
		{
		}
	}
}
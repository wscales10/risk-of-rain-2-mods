using System.Windows.Controls;
using WPFApp.Controls.Wrappers;

namespace WPFApp.Controls
{
	public abstract class ControlBase : UserControl
	{
		protected ControlBase(NavigationContext navigationContext) => NavigationContext = navigationContext;

		public NavigationContext NavigationContext { get; }

		public virtual SaveResult TrySave()
		{
			SaveResult result = ShouldAllowExit();

			if (result.IsSuccess)
			{
				result.Release();
			}

			return result;
		}

		protected virtual SaveResult ShouldAllowExit() => new(true);
	}
}
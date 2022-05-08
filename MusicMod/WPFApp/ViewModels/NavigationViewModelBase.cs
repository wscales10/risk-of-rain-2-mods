using WPFApp.Controls.Wrappers;

namespace WPFApp.ViewModels
{
	public abstract class NavigationViewModelBase : ViewModelBase
	{
		protected NavigationViewModelBase(NavigationContext navigationContext) => NavigationContext = navigationContext;

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

		public virtual string AsString => null;
	}
}
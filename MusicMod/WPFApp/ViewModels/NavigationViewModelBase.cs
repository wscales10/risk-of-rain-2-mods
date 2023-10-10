using WPFApp.Controls.Wrappers.SaveResults;

namespace WPFApp.ViewModels
{
    public abstract class NavigationViewModelBase : ViewModelBase
    {
        protected NavigationViewModelBase(NavigationContext navigationContext) => NavigationContext = navigationContext;

        public NavigationContext NavigationContext { get; }

        public virtual string AsString => null;

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
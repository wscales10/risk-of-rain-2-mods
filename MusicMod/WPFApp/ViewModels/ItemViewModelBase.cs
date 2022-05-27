using System;
using WPFApp.SaveResults;

namespace WPFApp.ViewModels
{
    public abstract class ItemViewModelBase : NavigationViewModelBase, IItemViewModel
    {
        protected ItemViewModelBase(NavigationContext navigationContext) : base(navigationContext)
        {
        }

        public event Action OnItemChanged;

        public abstract string ItemTypeName { get; }

        public abstract object ItemObject { get; }

        protected virtual bool Notify => true;

        public sealed override SaveResult TrySave()
        {
            SaveResult result = base.TrySave();

            if (Notify && result.IsSuccess)
            {
                OnItemChanged?.Invoke();
            }

            return result;
        }

        protected void NotifyItemChanged() => OnItemChanged?.Invoke();
    }
}
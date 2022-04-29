using System;
using WPFApp.Controls.Wrappers;

namespace WPFApp.Controls
{
	public abstract class ItemControlBase : ControlBase
	{
		protected ItemControlBase(NavigationContext navigationContext) : base(navigationContext)
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

	public abstract class ItemControlBase<TItem> : ItemControlBase
	{
		protected ItemControlBase(NavigationContext navigationContext) : base(navigationContext)
		{
		}

		public override sealed object ItemObject => Item;

		public abstract TItem Item { get; }

		public override string ItemTypeName => typeof(TItem).Name.ToLower();
	}
}
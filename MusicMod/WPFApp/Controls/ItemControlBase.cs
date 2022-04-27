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

		public sealed override SaveResult TrySave()
		{
			SaveResult result = base.TrySave();

			if (result)
			{
				OnItemChanged?.Invoke();
			}

			return result;
		}
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
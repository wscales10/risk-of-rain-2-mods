using System;
using System.Windows.Controls;

namespace WPFApp.Controls
{
	public abstract class ControlBase : UserControl
	{
		protected ControlBase(NavigationContext navigationContext) => NavigationContext = navigationContext;

		public event Action OnItemChanged;

		public NavigationContext NavigationContext { get; }

		public abstract string ItemTypeName { get; }

		public abstract object Object { get; }

		public bool TryExit()
		{
			if (ShouldAllowExit())
			{
				OnItemChanged?.Invoke();
				return true;
			}

			return false;
		}

		protected virtual bool ShouldAllowExit() => true;
	}

	public abstract class ControlBase<TItem> : ControlBase
	{
		protected ControlBase(NavigationContext navigationContext) : base(navigationContext)
		{
		}

		public override sealed object Object => Item;

		public abstract TItem Item { get; }

		public override string ItemTypeName => typeof(TItem).Name.ToLower();
	}
}
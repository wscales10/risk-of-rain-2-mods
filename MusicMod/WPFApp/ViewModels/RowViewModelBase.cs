using System.Collections.Generic;
using WPFApp.Controls.GridManagers;
using WPFApp.Controls.Rows;
using WPFApp.Controls.Wrappers;

namespace WPFApp.ViewModels
{
	public abstract class RowViewModelBase<TItem> : RowViewModelBase
	{
		protected RowViewModelBase(TItem item, NavigationContext navigationContext) : base(navigationContext) => Item = item;

		public sealed override object ItemObject => Item;

		public TItem Item { get; }

		public override string ItemTypeName => typeof(TItem).Name.ToLower();
	}

	public abstract class RowViewModelBase : ItemViewModelBase
	{
		protected RowViewModelBase(NavigationContext navigationContext) : base(navigationContext)
		{
			UpCommand = new(_ => RowManager.MoveSelected(false), () => RowManager.CanMoveSelected(false));
			DownCommand = new(_ => RowManager.MoveSelected(true), () => RowManager.CanMoveSelected(true));
			DeleteCommand = new(_ => RowManager.RemoveSelected(), () => RowManager.CanRemoveSelected());
			RowManager.SelectionChanged += UpdateButtons;
		}

		public ButtonCommand2 UpCommand { get; set; }

		public ButtonCommand2 DownCommand { get; set; }

		public ButtonCommand2 DeleteCommand { get; set; }

		public abstract IEnumerable<ButtonContext> ExtraCommands { get; }

		public IRowManager RowManager => TypedRowManager;

		protected abstract IRowManager TypedRowManager { get; }

		public void JointSelect(IRow row) => RowManager.ToggleSelected(row);

		public void Select(IRow row) => RowManager.SetSelection(row);

		public void UpdateButtons()
		{
			UpCommand.UpdateCanExecute();
			DownCommand.UpdateCanExecute();
			DeleteCommand.UpdateCanExecute();
		}

		protected override SaveResult ShouldAllowExit() => base.ShouldAllowExit() & RowManager.TrySaveChanges();
	}
}
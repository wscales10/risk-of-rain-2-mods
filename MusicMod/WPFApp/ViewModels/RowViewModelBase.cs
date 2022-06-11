using System.Collections.Generic;
using Utils;
using WPFApp.Controls.GridManagers;
using WPFApp.Controls.Rows;
using WPFApp.Controls.Wrappers.SaveResults;

namespace WPFApp.ViewModels
{
    public abstract class RowViewModelBase<TItem> : RowViewModelBase
    {
        protected RowViewModelBase(TItem item, NavigationContext navigationContext) : base(navigationContext) => Item = item;

        public sealed override object ItemObject => Item;

        public TItem Item { get; }

        public override string ItemTypeName => HelperMethods.AddSpacesToPascalCaseString(typeof(TItem).GetDisplayName(false));

        protected override SaveResult<TItem> ShouldAllowExit() => new(base.ShouldAllowExit(), Item);
    }

    public abstract class RowViewModelBase : ItemViewModelBase, INameableViewModel
    {
        protected RowViewModelBase(NavigationContext navigationContext) : base(navigationContext)
        {
            UpCommand = new(_ => RowManager.MoveSelected(false), () => RowManager.CanMoveSelected(false));
            DownCommand = new(_ => RowManager.MoveSelected(true), () => RowManager.CanMoveSelected(true));
            DeleteCommand = new(_ => RowManager.RemoveSelected(), () => RowManager.CanRemoveSelected());
            DuplicateCommand = new(_ => RowManager.DuplicateSelected(), () => RowManager.CanDuplicateSelected());
            RowManager.SelectionChanged += UpdateButtons;
            SetPropertyDependency(nameof(Name), NameResult, nameof(NameResult.Value));
        }

        public virtual string Title => string.Empty;

        public ButtonCommand2 UpCommand { get; }

        public ButtonCommand2 DownCommand { get; }

        public ButtonCommand2 DeleteCommand { get; }

        public ButtonCommand2 DuplicateCommand { get; }

        public abstract IEnumerable<ButtonContext> ExtraCommands { get; }

        public IRowManager RowManager => TypedRowManager;

        public string Name
        {
            get => NameResult?.Value;

            set
            {
                if (NameResult is not null)
                {
                    var name = value?.Trim();
                    NameResult.Value = name?.Length == 0 ? null : name;
                }
            }
        }

        public MutableSaveResultBase<string> NameResult { get; } = new StaticMutableSaveResult<string>();

        public virtual string NameWatermark => null;

        protected abstract IRowManager TypedRowManager { get; }

        public void JointSelect(IRow row) => RowManager.ToggleSelected(row);

        public void Select(IRow row) => RowManager.SetSelection(row);

        public void UpdateButtons()
        {
            UpCommand.UpdateCanExecute();
            DownCommand.UpdateCanExecute();
            DeleteCommand.UpdateCanExecute();
            DuplicateCommand.UpdateCanExecute();
        }

        protected override SaveResult ShouldAllowExit() => base.ShouldAllowExit() & RowManager.TrySaveChanges();
    }
}
using Rules.RuleTypes.Mutable;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Utils;
using WPFApp.Controls.Rows;

namespace WPFApp.Controls.RuleControls
{
	public abstract class RuleControlBase : XmlControlBase<Rule>, ITreeItem<RuleControlBase>, ITreeItem
	{
		protected RuleControlBase(NavigationContext navigationContext) : base(navigationContext)
		{
		}

		public virtual ReadOnlyObservableCollection<(string, RuleControlBase)> Children { get; } = new(new(Enumerable.Empty<(string, RuleControlBase)>()));

		IEnumerable<(string, ITreeItem)> ITreeItem.Children => Children.Select(p => (p.Item1, (ITreeItem)p.Item2));

		IEnumerable<(string, RuleControlBase)> ITreeItem<RuleControlBase>.Children => Children;

		internal void AttachRowEventHandlers<T>(RuleRow<T> row, bool _)
										where T : RuleRow<T>
		{
			row.OnOutputControlRequested += NavigationContext.GetControl;
			row.AllChildren = (NavigationContext.GetControl(row.Output) as IRowControl)?.RowManager.Rows;
			row.OnOutputButtonClick += NavigationContext.GoInto;
		}
	}
}
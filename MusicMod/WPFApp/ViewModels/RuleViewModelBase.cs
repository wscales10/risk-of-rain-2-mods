using Rules.RuleTypes.Mutable;
using System.Xml.Linq;
using WPFApp.Controls.Rows;

namespace WPFApp.ViewModels
{
	public abstract class RuleViewModelBase<TRule> : RowViewModelBase<TRule>, IXmlViewModel
		where TRule : Rule
	{
		protected RuleViewModelBase(TRule rule, NavigationContext navigationContext) : base(rule, navigationContext)
		{
		}

		public XElement GetContentXml() => Item.ToXml();

		internal void AttachRowEventHandlers<T>(RuleRow<T> row, bool _, int __)
										where T : RuleRow<T>
		{
			row.OutputViewModelRequestHandler = NavigationContext.GetViewModel;
			row.OnOutputButtonClick += NavigationContext.GoInto;
		}
	}
}
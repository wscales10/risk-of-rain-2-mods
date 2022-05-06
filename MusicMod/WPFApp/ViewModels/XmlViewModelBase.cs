using System.Xml.Linq;
using Utils;

namespace WPFApp.ViewModels
{
	public abstract class XmlViewModelBase<TItem> : RowViewModelBase<TItem>, IXmlViewModel
		where TItem : IXmlExportable
	{
		protected XmlViewModelBase(NavigationContext navigationContext) : base(navigationContext)
		{
		}

		public XElement GetContentXml() => Item.ToXml();
	}
}
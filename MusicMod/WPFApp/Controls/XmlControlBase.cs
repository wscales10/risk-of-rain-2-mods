using System.Xml.Linq;
using Utils;

namespace WPFApp.Controls
{
	public abstract class XmlControlBase<TItem> : ControlBase<TItem>, IXmlControl
		where TItem : IXmlExportable
	{
		protected XmlControlBase(NavigationContext navigationContext) : base(navigationContext)
		{
		}

		public XElement GetContentXml() => Item.ToXml();
	}
}
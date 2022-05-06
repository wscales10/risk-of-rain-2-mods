using System.Xml.Linq;

namespace WPFApp.ViewModels
{
	public interface IXmlViewModel : IItemViewModel
	{
		XElement GetContentXml();
	}
}
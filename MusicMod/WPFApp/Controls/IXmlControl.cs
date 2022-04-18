using System.Xml.Linq;

namespace WPFApp.Controls
{
	public interface IXmlControl
	{
		XElement GetContentXml();
	}
}
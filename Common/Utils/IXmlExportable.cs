using System;
using System.Xml.Linq;

namespace Utils
{
	public interface IXmlExportable
	{
		XElement ToXml();
	}

	public interface IXmlExportable<T>
	{
		XElement ToXml(Func<T, XElement> outFunc);
	}
}

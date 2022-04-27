using System;
using System.Xml.Linq;
using WPFApp.Controls.Wrappers;

namespace WPFApp.Controls
{
	public interface IItemControl
	{
		event Action OnItemChanged;

		string ItemTypeName { get; }

		object ItemObject { get; }

		SaveResult TrySave();
	}

	public interface IXmlControl : IItemControl
	{
		XElement GetContentXml();
	}
}
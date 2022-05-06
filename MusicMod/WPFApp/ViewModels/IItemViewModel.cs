using System;
using WPFApp.Controls.Wrappers;

namespace WPFApp.ViewModels
{
	public interface IItemViewModel
	{
		event Action OnItemChanged;

		string ItemTypeName { get; }

		object ItemObject { get; }

		SaveResult TrySave();
	}
}
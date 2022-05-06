using System;
using System.Windows;
using WPFApp.Controls.Wrappers;

namespace WPFApp.Controls.Rows
{
	public interface IRow
	{
		event Action<UIElement, UIElement> OnUiChanged;

		UIElement LeftElement { get; }

		bool IsMovable { get; }

		bool IsRemovable { get; }

		bool IsSelected { get; set; }

		UIElement RightElement { get; }

		SaveResult TrySaveChanges();
	}
}
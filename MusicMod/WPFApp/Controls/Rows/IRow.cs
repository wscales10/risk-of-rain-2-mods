using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using WPFApp.Controls.Wrappers;

namespace WPFApp.Controls.Rows
{
	public interface IRow
	{
		event Action<UIElement, UIElement> OnUiChanged;

		Border Border { get; }

		Border Background { get; }

		IEnumerable<UIElement> Elements { get; }

		UIElement LeftElement { get; }

		bool IsMovable { get; }

		bool IsRemovable { get; }

		SaveResult TrySaveChanges();
	}
}
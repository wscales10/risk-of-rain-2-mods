using System;
using System.ComponentModel;
using System.Windows;
using WPFApp.Controls.Wrappers.SaveResults;

namespace WPFApp.Controls.Rows
{
	public interface IRow : INotifyPropertyChanged
	{
		UIElement LeftElement { get; }

		bool IsMovable { get; }

		bool IsRemovable { get; }

		bool IsSelected { get; set; }

		UIElement RightElement { get; }

		SaveResult TrySaveChanges();
	}

	public interface IRow<out TOut> : IRow
	{
		event Action<TOut> OnSetOutput;

		TOut Output { get; }
	}
}
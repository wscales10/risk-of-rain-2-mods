using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WPFApp.Controls.Rows;
using WPFApp.Controls.Wrappers;

namespace WPFApp.Controls.GridManagers
{
	public interface IRowManager
	{
		event Action SelectionChanged;

		IRow Parent { get; set; }

		ReadOnlyObservableCollection<IRow> Rows { get; }

		IReadOnlyCollection<IRow> SelectedRows { get; }

		SaveResult TrySaveChanges();

		void MoveSelected(bool down);

		bool CanMoveSelected(bool down);

		bool CanRemoveSelected();

		void RemoveSelected();

		bool ToggleSelected(IRow row);

		void SetSelection(params IRow[] rows);
	}
}
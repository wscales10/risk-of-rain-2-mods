using System;
using System.Collections.Generic;
using WPFApp.Controls.Rows;
using WPFApp.Controls.Wrappers;

namespace WPFApp.Controls.GridManagers
{
	internal interface IRowManager
	{
		event Action SelectionChanged;

		IEnumerable<IRow> Rows { get; }

		IReadOnlyCollection<IRow> SelectedRows { get; }

		SaveResult TrySaveChanges();

		void MoveSelected(bool down);

		bool CanMoveSelected(bool down);

		bool CanRemoveSelected();

		void RemoveSelected();
	}
}
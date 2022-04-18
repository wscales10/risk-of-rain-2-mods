using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using WPFApp.Controls.Rows;

namespace WPFApp.Controls.GridManagers
{
	internal class RowManager<TRow> : GridManager<TRow>
		where TRow : Row<TRow>
	{
		public RowManager(Grid grid, Button addDefaultButton = null) : base(grid, addDefaultButton)
		{
		}

		protected override double RowMinHeight => 130;

		public bool TrySaveChanges()
		{
			bool success = true;

			foreach (TRow row in Rows)
			{
				if (!row.TrySaveChanges())
				{
					success = false;
				}
			}

			return success;
		}

		protected override IEnumerable<UIElement> GetUIElements(TRow item) => item.Elements;

		protected override void RefreshUi(TRow item, bool isAtTop, bool isAtBottom) => item.RefreshButtons(isAtTop, isAtBottom);

		protected override int add(TRow row, bool isDefault)
		{
			int node = base.add(row, isDefault);

			row.OnUiChanged += (oldElement, newElement) =>
			{
				int rowIndex;

				if (oldElement is null)
				{
					rowIndex = GetIndex(row);
				}
				else
				{
					rowIndex = Grid.GetRow(oldElement);
					Grid.Children.Remove(oldElement);
				}

				if (newElement is not null)
				{
					_ = Grid.Children.Add(newElement);
					Grid.SetRow(newElement, rowIndex);
				}
			};

			// TODO: Do I really want to do this even if the row isn't movable / removable?
			row.OnDelete += (node) => RemoveAt(node);
			row.OnMoveUp += (node) => Move(node, false);
			row.OnMoveDown += (node) => Move(node, true);

			return node;
		}
	}
}
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WPFApp.Controls.Rows;
using System.Collections.Specialized;
using System.Windows.Media;
using Utils;

namespace WPFApp.Controls.GridManagers
{
	internal class RowManager<TRow> : GridManager<TRow>, IRowManager where TRow : Row<TRow>
	{
		private readonly ObservableHashSet<int> selectedIndices = new();

		public RowManager(Grid grid, Button addDefaultButton = null) : base(grid, addDefaultButton) => selectedIndices.CollectionChanged += SelectedIndices_CollectionChanged;

		public event Action SelectionChanged;

		IEnumerable<IRow> IRowManager.Rows => Items;

		public IReadOnlyCollection<IRow> SelectedRows => selectedIndices.Select(i => List[i]).ToReadOnlyCollection();

		protected override double RowMinHeight => 130;

		public bool TrySaveChanges()
		{
			bool success = true;

			foreach (TRow row in Items)
			{
				if (!row.TrySaveChanges())
				{
					success = false;
				}
			}

			return success;
		}

		public void Select(int index) => selectedIndices.Add(index);

		public void SetSelection(params int[] indices)
		{
			selectedIndices.RemoveWhere(i => !indices.Contains(i));

			foreach (int i in indices)
			{
				selectedIndices.Add(i);
			}
		}

		public void SelectAll()
		{
			for (int i = 0; i < List.Count; i++)
			{
				_ = selectedIndices.Add(i);
			}
		}

		public void Unselect(int index) => selectedIndices.Remove(index);

		public void SelectNone() => selectedIndices.Clear();

		public void RemoveSelected()
		{
			while (selectedIndices.Count > 0)
			{
				int index = selectedIndices.First();
				RemoveAt(index);
			}
		}

		public override void RemoveAt(int index)
		{
			selectedIndices.Remove(index);
			base.RemoveAt(index);
		}

		public void MoveSelected(bool down)
		{
			foreach (int index in down
				? selectedIndices.OrderByDescending(i => i)
				: selectedIndices.OrderBy(i => i))
			{
				if (Move(index, down) is null)
				{
					return;
				}
			}
		}

		public override int? Move(int index, bool down)
		{
			if (base.Move(index, down) is not int newIndex)
			{
				return null;
			}

			ReplaceSelectedIndex(index, newIndex);
			return newIndex;
		}

		public bool CanMoveSelected(bool down)
		{
			if (selectedIndices.Count == 0)
			{
				return false;
			}

			if (!selectedIndices.All(i => IsMovable(List[i])))
			{
				return false;
			}

			if (IsMoveBlocked(down ? selectedIndices.Max() : selectedIndices.Min(), down))
			{
				return false;
			}

			return true;
		}

		public bool CanRemoveSelected() => selectedIndices.Count > 0 && selectedIndices.All(i => IsRemovable(List[i]));

		protected override bool IsRemovable(TRow row) => row.IsRemovable;

		protected override bool IsMovable(TRow row) => row.IsMovable;

		protected override void shift(int index, bool down)
		{
			base.shift(index, down);

			if (selectedIndices.Remove(index) && !selectedIndices.Add(index + (down ? 1 : -1)))
			{
				throw new InvalidOperationException("You're trying to shift rows in the wrong order.");
			}
		}

		protected override IEnumerable<UIElement> GetUIElements(TRow item) => item.Elements;

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

			row.JointSelected += () => ToggleSelected(Grid.GetRow(row.Background));
			row.Selected += () => SetSelection(Grid.GetRow(row.Background));
			return node;
		}

		private void ReplaceSelectedIndex(int rowOldIndex, int rowNewIndex)
		{
			bool rowIsSelected = selectedIndices.Contains(rowOldIndex);
			bool neighbourIsSelected = selectedIndices.Contains(rowNewIndex);

			if (rowIsSelected && !neighbourIsSelected)
			{
				selectedIndices.Add(rowNewIndex);
				selectedIndices.Remove(rowOldIndex);
			}

			if (neighbourIsSelected && !rowIsSelected)
			{
				selectedIndices.Add(rowOldIndex);
				selectedIndices.Remove(rowNewIndex);
			}
		}

		private bool ToggleSelected(int index)
		{
			if (selectedIndices.Contains(index))
			{
				selectedIndices.Remove(index);
				return false;
			}
			else
			{
				selectedIndices.Add(index);
				return true;
			}
		}

		private void SelectedIndices_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.NewItems is not null)
			{
				foreach (object oldItem in e.NewItems)
				{
					RowAt(oldItem).Paint(Brushes.PaleTurquoise);
				}
			}

			if (e.OldItems is not null)
			{
				foreach (object oldItem in e.OldItems)
				{
					RowAt(oldItem).Paint(Brushes.White);
				}
			}

			SelectionChanged?.Invoke();

			TRow RowAt(object indexObject)
			{
				return List[(int)indexObject];
			}
		}
	}
}
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using WPFApp.Rows;
using System.Collections.Specialized;
using Utils;
using System.Collections.ObjectModel;
using WPFApp.SaveResults;

namespace WPFApp.Controls.GridManagers
{
    public class RowManager<TRow> : GridManager<TRow>, IRowManager where TRow : Row<TRow>
    {
        private readonly ObservableHashSet<int> selectedIndices = new();

        private IRow parent;

        public RowManager() => selectedIndices.CollectionChanged += SelectedIndices_CollectionChanged;

        public event Action SelectionChanged;

        public ReadOnlyObservableCollection<IRow> Rows => new MappedObservableCollection<TRow, IRow>(Items, r => r);

        public IReadOnlyCollection<IRow> SelectedRows => selectedIndices.Select(i => List[i]).ToReadOnlyCollection();

        public IRow Parent
        {
            get => parent;

            set
            {
                if (value is not null)
                {
                    if (parent is null)
                    {
                        OnItemAdded += RowManager_OnItemAdded;
                    }
                }
                else if (parent is not null)
                {
                    OnItemAdded -= RowManager_OnItemAdded;
                }

                SetProperty(ref parent, value);

                foreach (TRow row in Items)
                {
                    SetParent(row);
                }
            }
        }

        private int? MinSelectedIndex => selectedIndices?.Count > 0 ? selectedIndices.Min() : null;

        public override TRow Add(TRow row) => Add(row, false, MinSelectedIndex);

        public override TDerived Add<TDerived>(TDerived row) => Add(row, false, MinSelectedIndex);

        public override SaveResult TrySaveChanges() => base.TrySaveChanges() & Items.All(r => r.TrySaveChanges());

        public void Select(int index) => selectedIndices.Add(index);

        public void SetSelection(params IRow[] rows)
        {
            var indices = rows.Cast<TRow>().Select(List.IndexOf).ToList();
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

        public bool ToggleSelected(IRow row)
        {
            int index = List.IndexOf((TRow)row);
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

        private void RowManager_OnItemAdded(TRow row, bool isDefault, int index) => SetParent(row);

        private void SetParent(TRow row)
        {
            if (row is IRuleRow ruleRow)
            {
                ruleRow.Parent = Parent as IRuleRow;
            }
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

        private void SelectedIndices_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems is not null)
            {
                foreach (object newItem in e.NewItems)
                {
                    RowAt(newItem).IsSelected = true;
                }
            }

            if (e.OldItems is not null)
            {
                foreach (object oldItem in e.OldItems)
                {
                    RowAt(oldItem).IsSelected = false;
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
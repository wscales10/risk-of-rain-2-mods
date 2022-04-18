using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Utils;
using WPFApp.Converters;

namespace WPFApp.Controls.GridManagers
{
	public abstract class GridManager<TRow> : INotifyPropertyChanged
		where TRow : class
	{
		private bool hasDefault;

		private bool added = true;

		protected GridManager(Grid grid, Button addDefaultButton = null)
		{
			Grid = grid;

			if (addDefaultButton is not null)
			{
				Binding myBinding = new(nameof(HasDefault))
				{
					Source = this,
					Converter = new InverseBooleanConverter()
				};

				_ = addDefaultButton.SetBinding(UIElement.IsEnabledProperty, myBinding);
			}
		}

		public delegate void RowMovedEventHandler(TRow row, int diff);

		public delegate void RowAddedEventHandler(TRow row, bool isDefault);

		public delegate void RowRemovedEventHandler(TRow row, bool wasDefault);

		public event RowAddedEventHandler OnRowAdded;

		public event RowMovedEventHandler OnRowMoved;

		public event RowRemovedEventHandler OnRowRemoved;

		public event PropertyChangedEventHandler PropertyChanged;

		public bool HasDefault
		{
			get => hasDefault;

			private set
			{
				hasDefault = value;
				OnPropertyChanged(nameof(HasDefault));
			}
		}

		public ReadOnlyCollection<TRow> Rows => List.ToReadOnlyCollection();

		protected Grid Grid { get; }

		protected List<TRow> List { get; } = new();

		protected abstract double RowMinHeight { get; }

		public int Add(TRow row) => Add(row, false);

		public int AddDefault(TRow row)
		{
			if (!HasDefault)
			{
				int index = Add(row, true);
				HasDefault = true;
				return index;
			}
			else
			{
				throw new InvalidOperationException();
			}
		}

		public int Move(int index, bool down)
		{
			if (HasDefault && index == List.Count - 1)
			{
				throw new InvalidOperationException();
			}

			int neighbouringIndex = down ? index + 1 : index - 1;

			if (neighbouringIndex < 0 || neighbouringIndex >= List.Count || (HasDefault && neighbouringIndex + 1 >= List.Count))
			{
				return index;
			}

			int diff = down ? 1 : -1;
			TRow row = List[index];
			TRow neighbour = List[neighbouringIndex];
			List.RemoveAt(index);
			List.Insert(neighbouringIndex, row);

			foreach (UIElement element in GetUIElements(row))
			{
				Grid.SetRow(element, Grid.GetRow(element) + diff);
			}

			foreach (UIElement element in GetUIElements(neighbour))
			{
				Grid.SetRow(element, Grid.GetRow(element) - diff);
			}

			RefreshUi(row, neighbouringIndex == 0, IsAtBottom(neighbouringIndex));
			RefreshUi(neighbour, index == 0, IsAtBottom(index));

			OnRowMoved?.Invoke(row, diff);
			return neighbouringIndex;
		}

		public void Remove(TRow item) => RemoveAt(List.IndexOf(item));

		public void RemoveAt(int index)
		{
			bool wasDefault;

			if (wasDefault = (index == List.Count - 1 && HasDefault))
			{
				HasDefault = false;
			}

			Grid.RowDefinitions.RemoveAt(0);
			TRow row = List[index];

			foreach (UIElement element in GetUIElements(row))
			{
				Grid.Children.Remove(element);
			}

			Shift(index + 1, false);
			List.RemoveAt(index);
			OnRowRemoved?.Invoke(row, wasDefault);
		}

		//#error doesn't work if you allow the value produced by valueGetter to change after the row has been added

		public void BindTo<T>(IList list, Func<T, TRow> rowMaker, Func<TRow, T> valueGetter, Action<TRow> setDefault = null)
		{
			foreach (T value in list)
			{
				added = false;
				var row = rowMaker(value);
				if (!added)
				{
					_ = Add(row);
				}
			}

			OnRowAdded += (row, isDefault) =>
			{
				if (isDefault)
				{
					setDefault(row);
				}
				else
				{
					list.Add(valueGetter(row));
				}
			};

			OnRowRemoved += (row, wasDefault) =>
			{
				if (wasDefault)
				{
					setDefault(null);
				}
				else
				{
					list.Remove(valueGetter(row));
				}
			};

			OnRowMoved += (row, diff) =>
			{
				var value = valueGetter(row);
				int i = list.IndexOf(value);
				list.RemoveAt(i);
				list.Insert(i + diff, value);
			};
		}

		protected int GetIndex(TRow row) => List.TakeWhile(r => !r.Equals(row)).Count();

		protected abstract IEnumerable<UIElement> GetUIElements(TRow item);

		protected virtual void RefreshUi(TRow item, bool isAtTop, bool isAtBottom)
		{ }

		protected virtual int add(TRow row, bool isDefault)
		{
			int targetIndex = List.Count;

			if (HasDefault)
			{
				targetIndex--;
			}

			Grid.RowDefinitions.Add(new RowDefinition { MinHeight = RowMinHeight, Height = GridLength.Auto });

			foreach (UIElement element in GetUIElements(row))
			{
				if (element is not null)
				{
					_ = Grid.Children.Add(element);
					Grid.SetRow(element, targetIndex);
				}
			}

			if (HasDefault)
			{
				_ = Shift(targetIndex, true);
				List.Insert(targetIndex, row);
			}
			else
			{
				List.Add(row);
			}

			RefreshUi(row, targetIndex == 0, true);

			if (targetIndex > 0)
			{
				RefreshUi(List[targetIndex - 1], targetIndex == 1, isDefault);
			}

			return targetIndex;
		}

		private bool Shift(int index, bool down)
		{
			if (index < 0 || index >= List.Count)
			{
				return false;
			}

			bool isAtTop = false, isAtBottom = false;

			if (down)
			{
				isAtBottom = !Shift(index + 1, true);
			}

			foreach (UIElement element in GetUIElements(List[index]))
			{
				Grid.SetRow(element, Grid.GetRow(element) + (down ? 1 : -1));
			}

			if (!down)
			{
				isAtTop = !Shift(index + 1, false);
			}

			RefreshUi(List[index], isAtTop, isAtBottom);
			return true;
		}

		private int Add(TRow row, bool isDefault)
		{
			int index = add(row, isDefault);
			OnRowAdded?.Invoke(row, isDefault);
			added = true;
			return index;
		}

		private bool IsAtBottom(int index)
		{
			return index == List.Count - 1 || (HasDefault && index == List.Count - 2);
		}

		private void OnPropertyChanged(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
	}
}
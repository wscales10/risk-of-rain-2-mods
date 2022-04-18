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

		protected LinkedList<TRow> List { get; } = new();

		protected abstract double RowMinHeight { get; }

		public LinkedListNode<TRow> Add(TRow row) => Add(row, false);

		public LinkedListNode<TRow> AddDefault(TRow row)
		{
			if (!HasDefault)
			{
				LinkedListNode<TRow> node = Add(row, true);
				HasDefault = true;
				return node;
			}

			return null;
		}

		public LinkedListNode<TRow> Move(LinkedListNode<TRow> node, bool down)
		{
			if (HasDefault && node == List.Last)
			{
				throw new InvalidOperationException();
			}

			LinkedListNode<TRow> neighbour = down ? node.Next : node.Previous;

			if (neighbour is null || (HasDefault && neighbour.Next is null))
			{
				return node;
			}

			int diff = down ? 1 : -1;
			TRow row = node.Value;
			List.Remove(node);
			node = down ? List.AddAfter(neighbour, row) : List.AddBefore(neighbour, row);

			foreach (UIElement element in GetUIElements(row))
			{
				Grid.SetRow(element, Grid.GetRow(element) + diff);
			}

			foreach (UIElement element in GetUIElements(neighbour.Value))
			{
				Grid.SetRow(element, Grid.GetRow(element) - diff);
			}

			RefreshUi(row, node.Previous is null, IsAtBottom(node));
			RefreshUi(neighbour.Value, neighbour.Previous is null, IsAtBottom(neighbour));

			OnRowMoved?.Invoke(row, diff);
			return node;
		}

		public void Remove(LinkedListNode<TRow> node)
		{
			bool wasDefault;

			if (wasDefault = HasDefault && node == List.Last)
			{
				HasDefault = false;
			}

			Grid.RowDefinitions.RemoveAt(0);
			TRow row = node.Value;

			foreach (UIElement element in GetUIElements(row))
			{
				Grid.Children.Remove(element);
			}

			Shift(node.Next, false);
			List.Remove(node);
			OnRowRemoved?.Invoke(row, wasDefault);
		}

#error doesn't work if you allow the value produced by valueGetter to change after the row has been added

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

		protected virtual LinkedListNode<TRow> add(TRow row, bool isDefault)
		{
			int rowIndex = List.Count;

			if (HasDefault)
			{
				rowIndex--;
			}

			Grid.RowDefinitions.Add(new RowDefinition { MinHeight = RowMinHeight, Height = GridLength.Auto });

			foreach (UIElement element in GetUIElements(row))
			{
				if (element is not null)
				{
					_ = Grid.Children.Add(element);
					Grid.SetRow(element, rowIndex);
				}
			}

			LinkedListNode<TRow> node;

			if (HasDefault)
			{
				LinkedListNode<TRow> defaultNode = List.Last;
				_ = Shift(defaultNode, true);
				node = List.AddBefore(defaultNode, row);
			}
			else
			{
				node = List.AddLast(row);
			}

			RefreshUi(row, rowIndex == 0, true);

			if (node.Previous is not null)
			{
				RefreshUi(node.Previous.Value, rowIndex - 1 == 0, isDefault);
			}

			return node;
		}

		private bool Shift(LinkedListNode<TRow> node, bool down)
		{
			if (node is null)
			{
				return false;
			}

			bool isAtTop = false, isAtBottom = false;

			if (down)
			{
				isAtBottom = !Shift(node.Next, true);
			}

			foreach (UIElement element in GetUIElements(node.Value))
			{
				Grid.SetRow(element, Grid.GetRow(element) + (down ? 1 : -1));
			}

			if (!down)
			{
				isAtTop = !Shift(node.Next, false);
			}

			RefreshUi(node.Value, isAtTop, isAtBottom);

			return true;
		}

		private LinkedListNode<TRow> Add(TRow row, bool isDefault)
		{
			LinkedListNode<TRow> node = add(row, isDefault);
			OnRowAdded?.Invoke(row, isDefault);
			added = true;
			return node;
		}

		private bool IsAtBottom(LinkedListNode<TRow> node)
		{
			LinkedListNode<TRow> next = node.Next;
			return next is null || (HasDefault && next.Next is null);
		}

		private void OnPropertyChanged(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
	}
}
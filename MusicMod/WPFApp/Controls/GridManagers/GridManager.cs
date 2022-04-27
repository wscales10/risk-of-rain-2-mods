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
using WPFApp.Controls.Wrappers;
using WPFApp.Converters;

namespace WPFApp.Controls.GridManagers
{
	public abstract class GridManager<TItem> : INotifyPropertyChanged
		where TItem : class
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

		public delegate void ItemMovedEventHandler(int index, int diff);

		public delegate void ItemAddedEventHandler(TItem row, bool isDefault);

		public delegate void ItemRemovedEventHandler(int index, bool wasDefault);

		public delegate SaveResult<T> ValueGetter<T>(TItem row);

		public delegate SaveResult DefaultSetter<T>(TItem row);

		public event ItemAddedEventHandler OnItemAdded;

		public event ItemMovedEventHandler OnItemMoved;

		public event ItemRemovedEventHandler OnItemRemoved;

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

		public ReadOnlyCollection<TItem> Items => List.ToReadOnlyCollection();

		public ReadOnlyCollection<TItem> ItemsWithoutDefault => (HasDefault ? List.Take(List.Count - 1) : List).ToReadOnlyCollection();

		public TItem DefaultItem => HasDefault ? List.Last() : default;

		protected Grid Grid { get; }

		protected List<TItem> List { get; } = new();

		protected abstract double RowMinHeight { get; }

		public TDerived Add<TDerived>(TDerived row)
			where TDerived : TItem
			=> Add(row, false);

		public TItem Add(TItem row) => Add(row, false);

		public TItem AddDefault(TItem row) => HasDefault ? throw new InvalidOperationException() : Add(row, true);

		public virtual int? Move(int index, bool down)
		{
			// Trying to move default
			if (HasDefault && index == List.Count - 1)
			{
				throw new InvalidOperationException();
			}

			int diff = down ? 1 : -1;
			int neighbouringIndex = index + diff;

			// Blocked
			if (IsMoveBlocked(index, down))
			{
				return null;
			}

			TItem row = List[index];
			TItem neighbour = List[neighbouringIndex];
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

			OnItemMoved?.Invoke(index, diff);
			return neighbouringIndex;
		}

		public void Remove(TItem item) => RemoveAt(List.IndexOf(item));

		public virtual void RemoveAt(int index)
		{
			bool wasDefault;

			if (wasDefault = index == List.Count - 1 && HasDefault)
			{
				HasDefault = false;
			}

			Grid.RowDefinitions.RemoveAt(0);
			TItem row = List[index];

			foreach (UIElement element in GetUIElements(row))
			{
				Grid.Children.Remove(element);
			}

			Shift(index + 1, false);
			List.RemoveAt(index);
			OnItemRemoved?.Invoke(index, wasDefault);
		}

		public Func<SaveResult> BindLooselyTo<T>(IList list, Func<T, TItem> rowMaker, ValueGetter<T> valueGetter, DefaultSetter<T> setDefault = null)
		{
			BindTo(list, rowMaker);

			OnItemAdded += (__, isDefault) =>
			{
				if (!isDefault)
				{
					_ = list.Add(default);
				}
			};

			OnItemRemoved += (index, wasDefault) =>
			{
				if (wasDefault)
				{
					setDefault(null);
				}
				else
				{
					list.RemoveAt(index);
				}
			};

			SaveResult TryGetValues()
			{
				SaveResult success = new(true);
				List<T> newList = new();

				foreach (TItem item in ItemsWithoutDefault)
				{
					var result = valueGetter(item);

					if (success.IsSuccess)
					{
						if (result.IsSuccess)
						{
							newList.Add(result.Value);
						}

						success &= result;
					}
				}

				list.Clear();

				foreach (T value in newList)
				{
					_ = list.Add(value);
				}

				if (HasDefault)
				{
					success &= setDefault(DefaultItem);
				}

				return success;
			}

			return TryGetValues;
		}

		public void BindTo<T>(IList list, Func<T, TItem> itemMaker, Func<TItem, T> valueGetter, Action<TItem> setDefault = null)
		{
			BindTo(list, itemMaker);

			OnItemRemoved += (index, wasDefault) =>
			{
				if (wasDefault)
				{
					setDefault(null);
				}
				else
				{
					list.RemoveAt(index);
				}
			};

			OnItemAdded += (item, isDefault) =>
			{
				if (isDefault)
				{
					setDefault(item);
				}
				else
				{
					list.Add(valueGetter(item));
				}
			};
		}

		protected virtual bool IsMovable(TItem row) => true;

		protected virtual bool IsRemovable(TItem row) => true;

		protected bool IsMoveBlocked(int index, bool down)
		{
			if (down)
			{
				for (int i = index + 1; i < List.Count; i++)
				{
					if (IsMovable(List[i]))
					{
						return false;
					}
				}
			}
			else
			{
				for (int i = index - 1; i >= 0; i--)
				{
					if (IsMovable(List[i]))
					{
						return false;
					}
				}
			}

			return true;
		}

		protected int GetIndex(TItem row) => List.TakeWhile(r => !r.Equals(row)).Count();

		protected abstract IEnumerable<UIElement> GetUIElements(TItem item);

		protected virtual int add(TItem row, bool isDefault)
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

			return targetIndex;
		}

		protected virtual void shift(int index, bool down)
		{
			foreach (UIElement element in GetUIElements(List[index]))
			{
				Grid.SetRow(element, Grid.GetRow(element) + (down ? 1 : -1));
			}
		}

		protected bool Shift(int index, bool down)
		{
			if (index < 0 || index >= List.Count)
			{
				return false;
			}

			if (down)
			{
				_ = Shift(index + 1, true);
			}

			shift(index, down);

			if (!down)
			{
				_ = Shift(index + 1, false);
			}

			return true;
		}

		private void BindTo<T>(IList list, Func<T, TItem> rowMaker)
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

			OnItemMoved += (index, diff) =>
			{
				var value = list[index];
				list.RemoveAt(index);
				list.Insert(index + diff, value);
			};
		}

		private TDerived Add<TDerived>(TDerived item, bool isDefault)
			where TDerived : TItem
		{
			int index = add(item, isDefault);
			OnItemAdded?.Invoke(item, isDefault);
			added = true;
			if (isDefault)
			{
				HasDefault = true;
			}
			return item;
		}

		private void OnPropertyChanged(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
	}
}
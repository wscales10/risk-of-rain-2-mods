using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Utils;
using WPFApp.Controls.Wrappers;

namespace WPFApp.Controls.GridManagers
{
	public abstract class GridManager<TItem> : NotifyPropertyChangedBase
		where TItem : class
	{
		private bool hasDefault;

		private bool added = true;

		private Func<SaveResult> tryGetValues;

		protected GridManager()
		{
		}

		public delegate void ItemMovedEventHandler(int index, int diff);

		public delegate void ItemAddedEventHandler(TItem row, bool isDefault, int index);

		public delegate void ItemRemovedEventHandler(int index, bool wasDefault);

		public delegate SaveResult<T> ValueGetter<T>(TItem row);

		public delegate SaveResult DefaultSetter<T>(TItem row);

		public event ItemAddedEventHandler OnItemAdded;

		public event ItemAddedEventHandler BeforeItemAdded;

		public event ItemMovedEventHandler OnItemMoved;

		public event ItemRemovedEventHandler OnItemRemoved;

		public bool HasDefault
		{
			get => hasDefault;

			private set => SetProperty(ref hasDefault, value);
		}

		public ReadOnlyObservableCollection<TItem> Items => new(List);

		public ReadOnlyCollection<TItem> ItemsWithoutDefault => (HasDefault ? List.Take(List.Count - 1) : List).ToReadOnlyCollection();

		public TItem DefaultItem => HasDefault ? List.Last() : default;

		protected ObservableCollection<TItem> List { get; } = new();

		public virtual TDerived Add<TDerived>(TDerived row)
			where TDerived : TItem
			=> Add(row, false);

		public virtual SaveResult TrySaveChanges() => tryGetValues is null ? new(true) : tryGetValues();

		public virtual TItem Add(TItem row) => Add(row, false);

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

			TItem row = List[index];
			Shift(index + 1, false);
			List.RemoveAt(index);
			OnItemRemoved?.Invoke(index, wasDefault);
		}

		public void BindLooselyTo<T>(IList list, Func<T, TItem> rowMaker, ValueGetter<T> valueGetter, DefaultSetter<T> setDefault = null)
		{
			BindTo(list, rowMaker);

			OnItemAdded += (__, isDefault, index) =>
			{
				if (!isDefault)
				{
					list.Insert(index, default);
				}
			};

			OnItemRemoved += (index, wasDefault) =>
			{
				if (wasDefault)
				{
					_ = setDefault(null);
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
					SaveResult<T> result = valueGetter(item);

					if (success.IsSuccess)
					{
						if (result.IsSuccess)
						{
							newList.Add(result.Value);
						}

						success &= result;
					}
				}

				if (HasDefault)
				{
					success &= setDefault(DefaultItem);
				}

				if (success)
				{
					list.Clear();

					foreach (T value in newList)
					{
						_ = list.Add(value);
					}
				}

				return success;
			}

			tryGetValues = TryGetValues;
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

			OnItemAdded += (item, isDefault, index) =>
			{
				if (isDefault)
				{
					setDefault(item);
				}
				else
				{
					list.Insert(index, valueGetter(item));
				}
			};

			SaveResult TryGetValues()
			{
				if (HasDefault)
				{
					setDefault(DefaultItem);
				}

				list.Clear();

				foreach (T value in ItemsWithoutDefault.Select(valueGetter))
				{
					_ = list.Add(value);
				}

				return new(true);
			}

			tryGetValues = TryGetValues;
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

		protected virtual void shift(int index, bool down)
		{
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

		protected TDerived Add<TDerived>(TDerived item, bool isDefault, int? targetIndex = null)
			where TDerived : TItem
		{
			int index = targetIndex ?? (HasDefault ? List.Count - 1 : List.Count);
			BeforeItemAdded?.Invoke(item, isDefault, index);

			if (index == List.Count)
			{
				List.Add(item);
			}
			else
			{
				List.Insert(index, item);
				_ = Shift(index, true);
			}

			OnItemAdded?.Invoke(item, isDefault, index);
			added = true;
			if (isDefault)
			{
				HasDefault = true;
			}
			return item;
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
	}
}
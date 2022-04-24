using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace WPFApp
{
	internal class History<T> : NotifyPropertyChangedBase, IEnumerable<T>, INotifyCollectionChanged
		where T : IChange<T>
	{
		private readonly ObservableCollection<T> changes = new();

		public event NotifyCollectionChangedEventHandler CollectionChanged
		{
			add => ((INotifyCollectionChanged)changes).CollectionChanged += value;

			remove => ((INotifyCollectionChanged)changes).CollectionChanged -= value;
		}

		public event Func<T, bool> ActionRequested;

		public int CurrentIndex { get; private set; }

		public int ReverseIndex => changes.Count - CurrentIndex;

		public IEnumerator<T> GetEnumerator() => changes.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)changes).GetEnumerator();

		public bool Try(T change, bool add)
		{
			if (Try(change))
			{
				for (int i = changes.Count - 1; i > CurrentIndex; i--)
				{
					changes.RemoveAt(i);
				}

				if (add)
				{
					changes.Add(change);
					CurrentIndex++;
				}

				return true;
			}

			return false;
		}

		public void Clear()
		{
			changes.Clear();
			CurrentIndex = 0;
		}

		public bool Redo(int count = 1)
		{
			for (int i = 0; i < count; i++)
			{
				if (ReverseIndex == 0 || !Try(changes[CurrentIndex]))
				{
					return false;
				}

				CurrentIndex++;
			}

			return true;
		}

		public bool Undo(int count = 1)
		{
			for (int i = 0; i < count; i++)
			{
				if (CurrentIndex == 0 || !Try(changes[CurrentIndex - 1].Reversed))
				{
					return false;
				}

				CurrentIndex--;
			}

			return true;
		}

		private bool Try(T change) => ActionRequested.Invoke(change);
	}
}
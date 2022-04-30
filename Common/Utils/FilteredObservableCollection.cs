using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Specialized;

namespace Utils
{
	public class FilteredObservableCollection<T> : ReadOnlyObservableCollection<T>
	{
		private readonly List<int> originalIndices = new List<int>();

		private readonly Dictionary<int, int> myIndices = new Dictionary<int, int>();

		private readonly Func<T, bool> predicate;

		public FilteredObservableCollection(ObservableCollection<T> list, Func<T, bool> predicate) : base(new ObservableCollection<T>())
		{
			list.CollectionChanged += List_CollectionChanged;
			this.predicate = predicate;
		}

		private ObservableCollection<T> InnerCollection => (ObservableCollection<T>)Items;

		private void TryInsert(int index, T value)
		{
			if (predicate(value))
			{
				if(myIndices)
			}
		}

		private void List_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					break;

				case NotifyCollectionChangedAction.Move:
					break;

				case NotifyCollectionChangedAction.Remove:
					break;

				case NotifyCollectionChangedAction.Replace:
					break;

				case NotifyCollectionChangedAction.Reset:
					break;
			}
		}
	}
}
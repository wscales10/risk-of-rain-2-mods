using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Utils
{
	public class CollectionChangeInfo<TIn>
	{
		private object dict;

		private IEnumerable<TIn> newItems;

		public CollectionChangeInfo(NotifyCollectionChangedEventArgs e)
		{
			NewStartingIndex = e.NewStartingIndex;
			OldStartingIndex = e.OldStartingIndex;
			NewItems = e.NewItems?.Cast<TIn>();
			OldItems = e.OldItems?.Cast<TIn>();
			Action = e.Action;
		}

		public NotifyCollectionChangedAction Action { get; }

		public int NewStartingIndex { get; set; }

		public int OldStartingIndex { get; set; }

		public virtual IEnumerable<TIn> NewItems
		{
			get => newItems;
			set => newItems = LazyList.Create(value);
		}

		public virtual IEnumerable<TIn> OldItems { get; set; }

		public void Apply<TOut>(ObservableCollection<TOut> InnerCollection, Func<TIn, TOut> map)
		{
			var dict = new Cache<TIn, TOut>(map);
			this.dict = dict;

			void Notifier_PropertyChanged(object sender, PropertyChangedEventArgs e)
			{
				var outItem = dict[(TIn)sender];
				InnerCollection[InnerCollection.IndexOf(outItem)] = outItem;
			}

			if (!(OldItems is null))
			{
				foreach (var item in OldItems)
				{
					if (item is INotifyPropertyChanged notifier)
					{
						notifier.PropertyChanged -= Notifier_PropertyChanged;
					}
				}
			}

			if (!(NewItems is null))
			{
				foreach (var item in NewItems)
				{
					if (item is INotifyPropertyChanged notifier)
					{
						notifier.PropertyChanged += Notifier_PropertyChanged;
					}
				}
			}

			apply(InnerCollection, map);
		}

		protected virtual void apply<TOut>(ObservableCollection<TOut> InnerCollection, Func<TIn, TOut> map)
		{
			var dict = (Cache<TIn, TOut>)this.dict;

			switch (Action)
			{
				case NotifyCollectionChangedAction.Add:
					InnerCollection.InsertRange(NewStartingIndex, dict[NewItems]);
					break;

				case NotifyCollectionChangedAction.Move:
					InnerCollection.RemoveRange(OldStartingIndex, OldItems.Count());
					InnerCollection.InsertRange(NewStartingIndex, dict[NewItems]);
					break;

				case NotifyCollectionChangedAction.Remove:
					InnerCollection.RemoveRange(OldStartingIndex, OldItems.Count());
					break;

				case NotifyCollectionChangedAction.Replace:
					InnerCollection.ReplaceRange(NewStartingIndex, dict[NewItems]);
					break;

				case NotifyCollectionChangedAction.Reset:
					InnerCollection.Clear();
					break;
			}
		}
	}
}
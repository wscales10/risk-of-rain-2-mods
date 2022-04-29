using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Utils
{
	public interface IReadOnlyObservableCollection<T> : IReadOnlyList<T>, INotifyCollectionChanged, INotifyPropertyChanged
	{
		int IndexOf(T value);
	}

	public interface IObservableCollection<T> : IReadOnlyObservableCollection<T>, IList<T>
	{
	}

	public static class MappedObservableCollection
	{
		public static FlattenedObservableCollection<T> Create<T>(IReadOnlyObservableCollection<ReadOnlyObservableCollection<T>> source) => new FlattenedObservableCollection<T>(source);

		public static MappedObservableCollection<TIn, TOut> Create<TIn, TOut>(ReadOnlyObservableCollection<TIn> source, Func<TIn, TOut> map) => new MappedObservableCollection<TIn, TOut>(source, map);
	}

	public abstract class MappedObservableCollection<TOut> : ReadOnlyObservableCollection<TOut>, IReadOnlyObservableCollection<TOut>
	{
		protected MappedObservableCollection(IEnumerable<TOut> collection) : base(new ObservableCollection<TOut>(collection))
		{
		}

		protected ObservableCollection<TOut> InnerCollection => (ObservableCollection<TOut>)Items;

		protected void HandleCollectionChange<T>(CollectionChangeInfo<T> info, Func<T, TOut> map) => info.Apply(InnerCollection, map);

		protected void HandleCollectionChange(CollectionChangeInfo<TOut> info) => HandleCollectionChange(info, x => x);
	}

	public class MappedObservableCollection<TIn, TOut> : MappedObservableCollection<TOut>
	{
		private readonly Func<TIn, TOut> map;

		public MappedObservableCollection(ReadOnlyObservableCollection<TIn> source, Func<TIn, TOut> map) : base(source.Select(map))
		{
			((INotifyCollectionChanged)source).CollectionChanged += Source_CollectionChanged;
			this.map = map;
		}

		private void Source_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			var info = new CollectionChangeInfo<TIn>(e)
			{
				NewItems = e.NewItems.Cast<TIn>(),
			};

			HandleCollectionChange(info, map);
		}
	}

	public class ManyMappedObservableCollection<TIn, TOut> : MappedObservableCollection<TOut>
	{
		private readonly Func<TIn, TOut> map;

		public ManyMappedObservableCollection(IReadOnlyObservableCollection<IReadOnlyList<TIn>> source, Func<TIn, TOut> map) : base(source.SelectMany(x => x).Select(map))
		{
			source.CollectionChanged += Source_CollectionChanged;
			this.map = map;
		}

		private static int GetOverallIndex<T>(IEnumerable<IList<T>> source, int chunkIndex)
		{
			return source.Take(chunkIndex).Sum(chunk => chunk.Count);
		}

		private void Source_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			var typedSender = (ObservableCollection<IList<TIn>>)sender;

			var info = new SuperCollectionChangeInfo<TIn>(e)
			{
				NewCollections = e.NewItems.Cast<IReadOnlyList<TIn>>().ToList(),
				OldCollections = e.OldItems.Cast<IReadOnlyList<TIn>>().ToList(),
				NewStartingIndex = GetOverallIndex(typedSender, e.NewStartingIndex),
				OldStartingIndex = GetOverallIndex(typedSender, e.OldStartingIndex)
			};

			HandleCollectionChange(info, map);
		}
	}

	public class FlattenedObservableCollection<T> : MappedObservableCollection<T>
	{
		private readonly IReadOnlyObservableCollection<ReadOnlyObservableCollection<T>> source;

		public FlattenedObservableCollection(IReadOnlyObservableCollection<ReadOnlyObservableCollection<T>> source) : base(source.SelectMany(x => x))
		{
			source.CollectionChanged += Source_CollectionChanged;

			foreach (var subCollection in source)
			{
				((INotifyCollectionChanged)subCollection).CollectionChanged += SubCollection_CollectionChanged;
			}

			this.source = source;
		}

		private static int GetOverallIndex(IEnumerable<IReadOnlyList<T>> source, int chunkIndex)
		{
			return source.Take(chunkIndex).Sum(chunk => chunk.Count);
		}

		private void SubCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			var startingIndex = GetOverallIndex(source, source.IndexOf((ReadOnlyObservableCollection<T>)sender));
			var info = new CollectionChangeInfo<T>(e)
			{
				NewStartingIndex = startingIndex + e.NewStartingIndex,
				OldStartingIndex = startingIndex + e.OldStartingIndex
			};

			HandleCollectionChange(info);
		}

		private void Source_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			var typedSender = (IReadOnlyObservableCollection<ReadOnlyObservableCollection<T>>)sender;
			var newCollections = e.NewItems?.Cast<ReadOnlyObservableCollection<T>>().ToList();
			var oldCollections = e.OldItems?.Cast<ReadOnlyObservableCollection<T>>().ToList();
			var info = new SuperCollectionChangeInfo<T>(e)
			{
				NewCollections = newCollections,
				OldCollections = oldCollections,
				NewStartingIndex = GetOverallIndex(typedSender, e.NewStartingIndex),
				OldStartingIndex = GetOverallIndex(typedSender, e.OldStartingIndex)
			};

			if (!(newCollections is null))
			{
				foreach (var collection in newCollections)
				{
					((INotifyCollectionChanged)collection).CollectionChanged += SubCollection_CollectionChanged;
				}
			}

			if (!(oldCollections is null))
			{
				foreach (var collection in oldCollections)
				{
					((INotifyCollectionChanged)collection).CollectionChanged -= SubCollection_CollectionChanged;
				}
			}

			HandleCollectionChange(info);
		}
	}
}
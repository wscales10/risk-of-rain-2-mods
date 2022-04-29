using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Collections.ObjectModel;
using System;

namespace Utils
{
	public class SuperCollectionChangeInfo<T> : CollectionChangeInfo<T>
	{
		public SuperCollectionChangeInfo(NotifyCollectionChangedEventArgs e) : base(e)
		{
		}

		public IReadOnlyList<IReadOnlyList<T>> NewCollections { get; set; }

		public override IEnumerable<T> NewItems => NewCollections?.SelectMany(c => c);

		public IReadOnlyList<IReadOnlyList<T>> OldCollections { get; set; }

		public override IEnumerable<T> OldItems => OldCollections?.SelectMany(c => c);

		protected override void apply<TOut>(ObservableCollection<TOut> InnerCollection, Func<T, TOut> map)
		{
			switch (Action)
			{
				case NotifyCollectionChangedAction.Replace:
					for (int j = 0; j < NewCollections.Count; j++)
					{
						OldStartingIndex = InnerCollection.ReplaceRange(OldStartingIndex, NewCollections[j].Select(map), OldCollections[j].Count);
					}
					break;

				default:
					base.Apply(InnerCollection, map);
					break;
			}
		}
	}
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Utils
{
    public static class CollectionChanged
    {
        public static ReadOnlyCollection<T> GetOldValues<T>(object sender, NotifyCollectionChangedEventArgs e) => new ReadOnlyCollection<T>(getOldValues<T>(sender, e));

        public static IEnumerable<int> GetChangedIndices<T>(object sender, NotifyCollectionChangedEventArgs e)
        {
            int oldWholeCount = ((IEnumerable<T>)sender).Count();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    return Enumerable.Range(e.NewStartingIndex, oldWholeCount - e.NewStartingIndex + e.NewItems.Count);

                case NotifyCollectionChangedAction.Move:
                    int min = Math.Min(e.OldStartingIndex, e.NewStartingIndex);
                    int max = Math.Max(e.OldStartingIndex, e.NewStartingIndex) + e.NewItems.Count;
                    return Enumerable.Range(min, max);

                case NotifyCollectionChangedAction.Remove:
                    return Enumerable.Range(e.OldStartingIndex, oldWholeCount - e.OldStartingIndex);

                case NotifyCollectionChangedAction.Replace:
                    return Enumerable.Range(e.NewStartingIndex, e.NewItems.Count);

                case NotifyCollectionChangedAction.Reset:
                    return Enumerable.Range(e.OldStartingIndex, e.OldItems.Count);

                default:
                    throw new ArgumentOutOfRangeException(nameof(e));
            }
        }

        private static List<T> getOldValues<T>(object sender, NotifyCollectionChangedEventArgs e)
        {
            var newValues = ((IEnumerable<T>)sender).ToList();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    newValues.RemoveRange(e.NewStartingIndex, e.NewItems.Count);
                    break;

                case NotifyCollectionChangedAction.Move:
                    newValues.RemoveRange(e.NewStartingIndex, e.NewItems.Count);
                    newValues.InsertRange(e.OldStartingIndex, e.OldItems.Cast<T>());
                    break;

                case NotifyCollectionChangedAction.Remove:
                    newValues.InsertRange(e.OldStartingIndex, e.OldItems.Cast<T>());
                    break;

                case NotifyCollectionChangedAction.Replace:
                    newValues.ReplaceRange(e.NewStartingIndex, e.OldItems.Cast<T>());
                    break;

                case NotifyCollectionChangedAction.Reset:
                    newValues.AddRange(e.OldItems.Cast<T>());
                    break;
            }

            return newValues;
        }
    }
}
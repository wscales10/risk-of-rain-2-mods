using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Utils;
using Utils.Reflection.Properties;

namespace WPFApp
{
    public abstract class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        private readonly Cache<INotifyPropertyChanged, AutoInitialiseDictionary<string, HashSet<string>>> objectCache;

        private readonly Cache<INotifyCollectionChanged, HashSet<string>> collectionCache;

        protected NotifyPropertyChangedBase()
        {
            objectCache = new(obj =>
            {
                obj.PropertyChanged += Object_PropertyChanged;
                return new(set => set.Count == 0);
            },
            dict => dict.Count == 0,
            obj => obj.PropertyChanged -= Object_PropertyChanged);

            collectionCache = new(coll =>
            {
                coll.CollectionChanged += Collection_CollectionChanged;
                return new();
            },
            set => set.Count == 0,
            coll => coll.CollectionChanged -= Collection_CollectionChanged);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void SubscribeTo(INotifyPropertyChanged obj) => obj.PropertyChanged += (s, e) => NotifyPropertyChanged(e);

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null) => NotifyPropertyChanged(new PropertyChangedEventArgs(propertyName));

        protected void RemovePropertyDependency(string propertyName, params string[] dependentOn)
        {
            foreach (var pair in objectCache)
            {
                var source = pair.Key;
                var dict = pair.Value;

                foreach (string s in dependentOn.Where(dict.ContainsKey))
                {
                    _ = dict[s].Remove(propertyName);
                    _ = dict.TryRemove(s);

                    if (source.GetPropertyValue(s) is INotifyCollectionChanged collection)
                    {
                        _ = collectionCache[collection].Remove(propertyName);
                        _ = collectionCache.TryRemove(collection);
                    }
                }
            }
        }

        protected void RemovePropertyDependency(string propertyName, INotifyPropertyChanged source, params string[] dependentOn)
        {
            if (source is null)
            {
                return;
            }

            var dict = objectCache[source];

            foreach (HashSet<string> set in dict[dependentOn])
            {
                _ = set.Remove(propertyName);
            }

            dict.TryRemove(dependentOn);
            _ = objectCache.TryRemove(source);

            foreach (string s in dependentOn)
            {
                if (source.GetPropertyValue(s) is INotifyCollectionChanged collection)
                {
                    var set = collectionCache[collection];
                    _ = set.Remove(propertyName);
                    _ = collectionCache.TryRemove(collection);
                }
            }
        }

        protected void SetPropertyDependency(string propertyName, INotifyPropertyChanged source, params string[] dependentOn)
        {
            if (propertyName is null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            if (source is null)
            {
                return;
            }

            foreach (HashSet<string> set in objectCache[source][dependentOn])
            {
                _ = set.Add(propertyName);
            }

            foreach (string s in dependentOn)
            {
                if (source.GetPropertyValue(s) is INotifyCollectionChanged collection)
                {
                    _ = collectionCache[collection].Add(propertyName);
                }
            }
        }

        protected void SetPropertyDependency(string propertyName, params string[] dependentOn) => SetPropertyDependency(propertyName, this, dependentOn);

        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            field = value;
            NotifyPropertyChanged(propertyName);
        }

        private void Collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (string dependant in collectionCache[(INotifyCollectionChanged)sender])
            {
                NotifyPropertyChanged(dependant);
            }
        }

        private void Object_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            AutoInitialiseDictionary<string, HashSet<string>> dictionary = objectCache[(INotifyPropertyChanged)sender];
            if (dictionary.ContainsKey(e.PropertyName))
            {
                foreach (string dependant in dictionary[e.PropertyName])
                {
                    NotifyPropertyChanged(dependant);
                }

                if (typeof(INotifyCollectionChanged).IsAssignableFrom(sender.GetType().GetProperty(e.PropertyName).PropertyType))
                /*if (sender.TryGetPropertyValue<INotifyCollectionChanged>(e.PropertyName, out var collection))*/
                {
                    var collection = (INotifyCollectionChanged)sender.GetPropertyValue(e.PropertyName);
                    foreach (string dependant in objectCache[(INotifyPropertyChanged)sender][e.PropertyName])
                    {
                        _ = collectionCache[collection].Add(dependant);
                    }
                }
            }
        }

        private void NotifyPropertyChanged(PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);
    }
}
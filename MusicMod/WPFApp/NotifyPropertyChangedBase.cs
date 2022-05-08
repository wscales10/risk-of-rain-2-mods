using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Utils;
using Utils.Reflection.Properties;

namespace WPFApp
{
    public abstract class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        private readonly Cache<INotifyPropertyChanged, AutoInitialiseDictionary<string, HashSet<string>>> cache;

        private readonly Cache<INotifyCollectionChanged, HashSet<string>> cache2;

        protected NotifyPropertyChangedBase()
        {
            cache = new(obj =>
            {
                obj.PropertyChanged += Obj_PropertyChanged;
                return new();
            });

            cache2 = new(coll =>
            {
                coll.CollectionChanged += Coll_CollectionChanged;
                return new();
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void SubscribeTo(INotifyPropertyChanged obj) => obj.PropertyChanged += (s, e) => NotifyPropertyChanged(e);

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null) => NotifyPropertyChanged(new PropertyChangedEventArgs(propertyName));

        protected void RemovePropertyDependency(string propertyName, INotifyPropertyChanged source, params string[] dependentOn)
        {
            if (source is null)
            {
                return;
            }

            foreach (HashSet<string> set in cache[source][dependentOn])
            {
                _ = set.Remove(propertyName);

                // TODO: remove HashSet and event handler if set is now empty
            }

            foreach (string s in dependentOn)
            {
                if (source.GetPropertyValue(s) is INotifyCollectionChanged collection)
                {
                    _ = cache2[collection].Remove(propertyName);

                    // TODO: remove HashSet and event handler if set is now empty
                }
            }
        }

        protected void SetPropertyDependency(string propertyName, INotifyPropertyChanged source, params string[] dependentOn)
        {
            if (source is null)
            {
                return;
            }

            foreach (HashSet<string> set in cache[source][dependentOn])
            {
                _ = set.Add(propertyName);
            }

            foreach (string s in dependentOn)
            {
                if (source.GetPropertyValue(s) is INotifyCollectionChanged collection)
                {
                    _ = cache2[collection].Add(propertyName);
                }
            }
        }

        protected void SetPropertyDependency(string propertyName, params string[] dependentOn) => SetPropertyDependency(propertyName, this, dependentOn);

        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            field = value;
            NotifyPropertyChanged(propertyName);
        }

        private void Coll_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (string dependant in cache2[(INotifyCollectionChanged)sender])
            {
                NotifyPropertyChanged(dependant);
            }
        }

        private void Obj_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            AutoInitialiseDictionary<string, HashSet<string>> dictionary = cache[(INotifyPropertyChanged)sender];
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
                    foreach (string dependant in cache[(INotifyPropertyChanged)sender][e.PropertyName])
                    {
                        _ = cache2[collection].Add(dependant);
                    }
                }
            }
        }

        private void NotifyPropertyChanged(PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;

namespace WPFApp.ViewModels
{
    public abstract class DiscreteMenuViewModel : ViewModelBase
    {
        public event Action<object> OnObjectSelected;

        public abstract IEnumerable ItemsSource { get; }

        public abstract ICommand Command { get; }

        public abstract bool HasItems { get; }

        protected void CommitSelection(object obj) => OnObjectSelected?.Invoke(obj);
    }

    public class DiscreteMenuViewModel<T> : DiscreteMenuViewModel
    {
        public DiscreteMenuViewModel(ICollection<T> itemsSource, string displayMemberPath = null)
        {
            ItemsSource = itemsSource;
            DisplayMemberPath = displayMemberPath;
            if (ItemsSource is INotifyPropertyChanged inpc)
            {
                SetPropertyDependency(nameof(HasItems), inpc, nameof(ItemsSource.Count));
            }

            Command = new ButtonCommand(o => CommitSelection((T)o));
            OnValueSelected += value => CommitSelection((object)value);
        }

        public event Action<T> OnValueSelected;

        public override ICollection<T> ItemsSource { get; }

        public string DisplayMemberPath { get; }

        public override bool HasItems => ItemsSource?.Count > 0;

        public override ICommand Command { get; }

        protected void CommitSelection(T value) => OnValueSelected?.Invoke(value);
    }
}
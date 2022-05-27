using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WPFApp.SaveResults;
using WPFApp.ViewModels;

namespace WPFApp.Rows
{
    public delegate int IndexGetter<in TRow>(TRow row);

    public delegate void RowSavedEventHandler<in TRow>(TRow sender, SaveResult result);

    public abstract class Row<TRow> : NotifyDataErrorInfoBase, IRow where TRow : Row<TRow>
    {
        private bool isSelected;

        protected Row(bool movable, bool removable = true)
        {
            IsMovable = movable;
            IsRemovable = removable;
        }

        public event RowSavedEventHandler<TRow> Saved;

        public bool IsMovable { get; }

        public bool IsRemovable { get; }

        public virtual UIElement LeftElement { get; } = new TextBlock() { TextAlignment = TextAlignment.Center, VerticalAlignment = VerticalAlignment.Center, FontSize = 14, Margin = new Thickness(4) };

        public ICollectionView Children => Row.Filter(AllChildren, r => (r as IRuleRow)?.Output is not null);

        public bool IsSelected
        {
            get => isSelected;
            set => SetProperty(ref isSelected, value);
        }

        protected virtual ReadOnlyObservableCollection<IRow> AllChildren { get; } = new(new());

        public SaveResult TrySaveChanges()
        {
            var result = trySaveChanges();
            Saved?.Invoke((TRow)this, result);
            return result;
        }

        protected virtual SaveResult trySaveChanges() => new(!HasErrors);
    }

    internal static class Row
    {
        internal static ICollectionView Filter<T>(ICollection<T> source, Func<T, bool> predicate)
        {
            ICollectionView output = CollectionViewSource.GetDefaultView(source);

            if (output is not null)
            {
                output.Filter = x => predicate((T)x);
            }

            return output;
        }
    }

    internal abstract class Row<TOut, TRow> : Row<TRow>
        where TOut : class
        where TRow : Row<TOut, TRow>
    {
        private TOut output;

        protected Row(bool movable, bool removable = true)
            : base(movable, removable)
        {
            PropertyChanged += Row_PropertyChanged;
        }

        public event Action<TOut> OnSetOutput;

        public virtual TOut Output
        {
            get => output;
            set => SetProperty(ref output, value);
        }

        protected override SaveResult trySaveChanges() => new SaveResult<TOut>(base.trySaveChanges(), Output);

        private void Row_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Output):
                    OnSetOutput?.Invoke(Output);
                    break;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WPFApp.Controls.Wrappers.SaveResults;

namespace WPFApp.Controls.Rows
{
	public delegate int IndexGetter<in TRow>(TRow row);

	public delegate void RowSavedEventHandler<in TRow>(TRow sender, SaveResult result);

	public abstract class Row<TRow> : NotifyPropertyChangedBase, IRow where TRow : Row<TRow>
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

		public virtual UIElement RightElement { get; protected set; }

		public virtual UIElement LeftElement { get; } = new TextBlock() { TextAlignment = TextAlignment.Center, VerticalAlignment = VerticalAlignment.Center, FontSize = 14, Margin = new Thickness(4) };

		public ICollectionView Children => Row.Filter(AllChildren, r => (r as IRuleRow)?.OutputViewModel is not null);

		public bool IsSelected
		{
			get => isSelected;
			set => SetProperty(ref isSelected, value);
		}

		protected virtual ReadOnlyObservableCollection<IRow> AllChildren { get; } = new(new());

		public abstract TRow DeepClone();

		public SaveResult TrySaveChanges()
		{
			var result = trySaveChanges();
			Saved?.Invoke((TRow)this, result);
			return result;
		}

		protected virtual SaveResult trySaveChanges() => new(true);
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

	internal abstract class Row<TOut, TRow> : Row<TRow>, IRow<TOut>
		where TOut : class
		where TRow : Row<TOut, TRow>
	{
		private TOut output;

		private UIElement rightElement;

		protected Row(bool movable, bool removable = true)
			: base(movable, removable)
		{
			PropertyChanged += Row_PropertyChanged;
		}

		public event Action<TOut> OnSetOutput;

		public sealed override UIElement RightElement
		{
			get => rightElement;
			protected set => SetProperty(ref rightElement, value);
		}

		public virtual TOut Output
		{
			get => output;
			set => SetProperty(ref output, value);
		}

		public sealed override TRow DeepClone()
		{
			var clone = deepClone();
			clone.Output = CloneOutput();
			return clone;
		}

		protected abstract TOut CloneOutput();

		protected abstract TRow deepClone();

		protected abstract UIElement MakeOutputUi();

		protected void RefreshOutputUi() => RefreshOutputUi(RightElement, Output);

		protected virtual void RefreshOutputUi(UIElement ui, TOut output)
		{ }

		private void Row_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case nameof(Output):
					RightElement = MakeOutputUi();
					RefreshOutputUi();
					OnSetOutput?.Invoke(Output);
					break;
			}
		}
	}
}
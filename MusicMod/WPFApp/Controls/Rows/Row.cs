using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WPFApp.Controls.Wrappers;

namespace WPFApp.Controls.Rows
{
	public delegate int IndexGetter<TRow>(TRow row);

	public abstract class Row<TRow> : NotifyPropertyChangedBase, IRow where TRow : Row<TRow>
	{
		private bool isSelected;

		protected Row(bool movable, bool removable = true)
		{
			IsMovable = movable;
			IsRemovable = removable;
		}

		public event Action<UIElement, UIElement> OnUiChanged;

		public bool IsMovable { get; }

		public bool IsRemovable { get; }

		public virtual UIElement RightElement => null;

		public virtual UIElement LeftElement { get; } = new TextBlock() { TextAlignment = TextAlignment.Center, VerticalAlignment = VerticalAlignment.Center, FontSize = 14, Margin = new Thickness(4) };

		public ICollectionView Children => Row.Filter(AllChildren, r => (r as IRuleRow)?.Output is not null);

		public bool IsSelected
		{
			get => isSelected;
			set => SetProperty(ref isSelected, value);
		}

		protected virtual ReadOnlyObservableCollection<IRow> AllChildren { get; } = new(new());

		public virtual SaveResult TrySaveChanges() => new(true);

		protected void PropagateUiChange(UIElement oldElement, UIElement newElement)
		{
			NotifyPropertyChanged(nameof(RightElement));
			OnUiChanged?.Invoke(oldElement, newElement);
		}
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
		private readonly CustomContentPresenter<TOut> outputPresenter = new();

		protected Row(TOut output, bool movable, bool removable = true)
			: base(movable, removable)
		{
			outputPresenter.OnContentModified += (o, u) => RefreshOutputUi(u, o);
			outputPresenter.OnUiRequested += MakeOutputUi;
			outputPresenter.OnSetContent += (o) => OnSetOutput?.Invoke(o);
			outputPresenter.OnUiChanged += PropagateUiChange;
			Output = output;
		}

		public event Action<TOut> OnSetOutput;

		public sealed override UIElement RightElement => outputPresenter.UI;

		public virtual TOut Output
		{
			get => outputPresenter.Content;

			protected set
			{
				outputPresenter.Content = value;
				NotifyPropertyChanged();
			}
		}

		protected abstract UIElement MakeOutputUi();

		protected void RefreshOutputUi() => outputPresenter.RefreshUi();

		protected virtual void RefreshOutputUi(UIElement ui, TOut output)
		{ }
	}
}
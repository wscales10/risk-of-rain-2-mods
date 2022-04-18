using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Utils;

namespace WPFApp.Controls.Rows
{
	public delegate int IndexGetter<TRow>(TRow row);

	internal abstract class Row<TRow>
		where TRow : Row<TRow>
	{
		private readonly ChangeProperty<bool> Movable = new();

		private readonly ChangeProperty<bool> Removable = new();

		private Brush brush;

		protected Row(IndexGetter<TRow> indexGetter, bool movable, bool removable = true)
		{
			Movable.OnChanged += (value) =>
			{
				if (value)
				{
					UpButton = new() { Content = "Up", Height = 50, MinWidth = 50, Margin = new Thickness(3) };
					DownButton = new() { Content = "Down", Height = 50, MinWidth = 50, Margin = new Thickness(3) };

					if (brush is not null)
					{
						UpButton.Background = brush;
						DownButton.Background = brush;
					}

					UpButton.Click += (s, e) => Node = OnMoveUp?.Invoke(Node.Value);
					DownButton.Click += (s, e) => Node = OnMoveDown?.Invoke(Node.Value);
					_ = ButtonsPanel.Children.Add(UpButton);
					_ = ButtonsPanel.Children.Add(DownButton);
				}
				else
				{
					ButtonsPanel.Children.Remove(UpButton);
					ButtonsPanel.Children.Remove(DownButton);
					UpButton = null;
					DownButton = null;
				}
			};

			Movable.Set(movable);

			Removable.OnChanged += (value) =>
			{
				if (value)
				{
					DeleteButton = new() { Content = "Delete", Height = 50, MinWidth = 50, Margin = new Thickness(3) };
					DeleteButton.Click += (s, e) => OnDelete?.Invoke(Node.Value);
					_ = ButtonsPanel.Children.Add(DeleteButton);
				}
				else
				{
					ButtonsPanel.Children.Remove(DeleteButton);
					DeleteButton = null;
				}
			};

			Removable.Set(removable);

			Grid.SetColumnSpan(Border, int.MaxValue);
			Grid.SetColumn(ButtonsPanel, 2);
			Node = indexGetter((TRow)this);
		}

		public event Action<int> OnDelete;

		public event Func<int, int> OnMoveDown;

		public event Func<int, int> OnMoveUp;

		public event Action<UIElement, UIElement> OnUiChanged;

		public Border Border { get; } = new Border { BorderThickness = new Thickness(1), BorderBrush = Brushes.Black, VerticalAlignment = VerticalAlignment.Bottom };

		public StackPanel ButtonsPanel { get; } = new() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(30) };

		public virtual IEnumerable<UIElement> Elements => new UIElement[] { Border, LeftElement, ButtonsPanel };

		public int? Node { get; private set; }

		public virtual UIElement LeftElement { get; } = new TextBlock() { TextAlignment = TextAlignment.Center, VerticalAlignment = VerticalAlignment.Center, FontSize = 14, Margin = new Thickness(4) };

		protected Button DeleteButton { get; private set; }

		protected Button DownButton { get; private set; }

		protected Button UpButton { get; private set; }

		public virtual bool TrySaveChanges() => true;

		public void Paint(Color color)
		{
			SolidColorBrush brush = new(color);

			if (Movable)
			{
				UpButton.Background = brush;
				DownButton.Background = brush;
			}
			else
			{
				this.brush = brush;
			}
		}

		public void RefreshButtons(bool isAtTop, bool isAtBottom)
		{
			if (Movable)
			{
				UpButton.IsEnabled = !isAtTop;
				DownButton.IsEnabled = !isAtBottom;
			}
		}

		protected void PropagateUiChange(UIElement oldElement, UIElement newElement) => OnUiChanged?.Invoke(oldElement, newElement);
	}

	internal abstract class Row<TOut, TRow> : Row<TRow>
			where TOut : class
		where TRow : Row<TOut, TRow>
	{
		private readonly CustomContentPresenter<TOut> outputPresenter = new();

		protected Row(TOut output, IndexGetter<TRow> indexGetter, bool movable, bool removable = true)
			: base(indexGetter, movable, removable)
		{
			outputPresenter.OnContentModified += (o, u) => RefreshOutputUi(u, o);
			outputPresenter.OnUiRequested += MakeOutputUi;
			outputPresenter.OnSetContent += (o) => OnSetOutput?.Invoke(o);
			outputPresenter.OnUiChanged += PropagateUiChange;
			Output = output;
		}

		public event Action<TOut> OnSetOutput;

		public override sealed IEnumerable<UIElement> Elements => base.Elements.Concat(new[] { outputPresenter.UI });

		public TOut Output
		{
			get => outputPresenter.Content;

			protected set => outputPresenter.Content = value;
		}

		protected abstract UIElement MakeOutputUi();

		protected void RefreshOutputUi() => outputPresenter.RefreshUi();

		protected virtual void RefreshOutputUi(UIElement ui, TOut output)
		{ }
	}
}
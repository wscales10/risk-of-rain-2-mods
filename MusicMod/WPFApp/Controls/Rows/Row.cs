using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WPFApp.Controls.Rows
{
	public delegate int IndexGetter<TRow>(TRow row);

	internal abstract class Row<TRow> : IRow where TRow : Row<TRow>
	{
		protected Row(bool movable, bool removable = true)
		{
			IsMovable = movable;
			IsRemovable = removable;
			Grid.SetColumnSpan(Border, int.MaxValue);
			Grid.SetColumn(ButtonsPanel, 2);
			Border.PreviewMouseLeftButtonDown += (s, e) =>
			{
				if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.LeftShift))
				{
					JointSelected?.Invoke();
				}
				else
				{
					Selected?.Invoke();
				}
			};
		}

		public event Action<UIElement, UIElement> OnUiChanged;

		public event Action Selected;

		public event Action JointSelected;

		public bool IsMovable { get; }

		public bool IsRemovable { get; }

		public Border Border { get; } = new Border { BorderThickness = new Thickness(1), BorderBrush = Brushes.Black, Background = Brushes.White };

		public StackPanel ButtonsPanel { get; } = new() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(30) };

		public virtual IEnumerable<UIElement> Elements => new UIElement[] { Border, LeftElement, ButtonsPanel };

		public virtual UIElement LeftElement { get; } = new TextBlock() { TextAlignment = TextAlignment.Center, VerticalAlignment = VerticalAlignment.Center, FontSize = 14, Margin = new Thickness(4) };

		public void Paint(Brush brush) => Border.Background = brush;

		public virtual bool TrySaveChanges() => true;

		protected void PropagateUiChange(UIElement oldElement, UIElement newElement) => OnUiChanged?.Invoke(oldElement, newElement);
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
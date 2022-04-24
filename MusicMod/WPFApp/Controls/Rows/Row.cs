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
			Grid.SetColumnSpan(Background, int.MaxValue);
			Grid.SetColumnSpan(Border, int.MaxValue);
			Background.PreviewMouseLeftButtonDown += (s, e) =>
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

		public Border Border { get; } = new Border { BorderThickness = new Thickness(1), BorderBrush = Brushes.Black, VerticalAlignment = VerticalAlignment.Bottom };

		public Border Background { get; } = new Border { Background = Brushes.White };

		public IEnumerable<UIElement> Elements => new UIElement[] { Background }.Concat(ExtraUi).Concat(new UIElement[] { LeftElement, Border });

		public virtual IEnumerable<UIElement> ExtraUi => Enumerable.Empty<UIElement>();

		public virtual UIElement LeftElement { get; } = new TextBlock() { TextAlignment = TextAlignment.Center, VerticalAlignment = VerticalAlignment.Center, FontSize = 14, Margin = new Thickness(4) };

		public void Paint(Brush brush) => Background.Background = brush;

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

		public override sealed IEnumerable<UIElement> ExtraUi => new[] { outputPresenter.UI };

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
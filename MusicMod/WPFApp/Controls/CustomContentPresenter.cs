using System;
using System.Windows;

namespace WPFApp.Controls
{
	internal class CustomContentPresenter<TContent>
	{
		private UIElement ui;

		private TContent content;

		public event Func<UIElement> OnUiRequested;

		public event Action<TContent> OnSetContent;

		public event Action<UIElement, UIElement> OnUiChanged;

		public event Action<TContent, UIElement> OnContentModified;

		public TContent Content
		{
			get => content;

			set
			{
				content = value;
				UI = OnUiRequested?.Invoke();
				RefreshUi();
				OnSetContent?.Invoke(value);
			}
		}

		public UIElement UI
		{
			get => ui;

			private set
			{
				UIElement oldElement = ui;
				PropagateUiChange(oldElement, ui = value);
			}
		}

		public void PropagateUiChange(UIElement oldElement, UIElement newElement) => OnUiChanged?.Invoke(oldElement, newElement);

		public void RefreshUi() => OnContentModified?.Invoke(content, UI);
	}
}
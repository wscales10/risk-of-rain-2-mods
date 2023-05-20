using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WPFApp.ViewModels;

namespace WPFApp.Controls.Rows
{
	internal class ButtonOutputUi<TOut> : NotifyPropertyChangedBase
		where TOut : class
	{
		private readonly NavigationContext navigationContext;

		private readonly Action refresh;

		private readonly IRow<TOut> parentRow;

		private readonly Func<string> getButtonContent;

		private Func<TOut, NavigationViewModelBase> outputViewModelRequestHandler;

		public ButtonOutputUi(NavigationContext navigationContext, Action refresh, IRow<TOut> parentRow, PropertyInformation<string> getButtonContent)
		{
			this.navigationContext = navigationContext;
			this.refresh = refresh;
			this.parentRow = parentRow;
			this.getButtonContent = getButtonContent.GetValue;
			OutputViewModelRequestHandler = navigationContext.GetViewModel;
			OnOutputButtonClick += (viewModel) => navigationContext.GoInto(viewModel);
			SetPropertyDependency(nameof(ButtonContent), nameof(OutputViewModel));

			foreach (var dependency in getButtonContent.Dependencies)
			{
				SetPropertyDependency(nameof(ButtonContent), dependency.Source ?? this, dependency.DependentOn);
			}

			parentRow.OnSetOutput += (output) =>
			{
				OutputViewModel = OutputViewModelRequestHandler?.Invoke(output);
			};
		}

		public event Action<NavigationViewModelBase> OnOutputButtonClick;

		public NavigationViewModelBase OutputViewModel
		{
			get => outputViewModel;

			private set
			{
				if (value is not null)
				{
					navigationContext.Register(value, parentRow as IRuleRow);
				}

				var oldValue = OutputViewModel;

				RemovePropertyDependency(nameof(ButtonContent), oldValue, nameof(NavigationViewModelBase.AsString));

				if (oldValue is ItemViewModelBase oldItemViewModel)
				{
					oldItemViewModel.OnItemChanged -= refresh;
				}

				if (value is ItemViewModelBase newItemViewModel)
				{
					newItemViewModel.OnItemChanged += refresh;
				}

				if (value is RowViewModelBase rowViewModel)
				{
					rowViewModel.RowManager.Parent = parentRow;
				}

				if (value is not null)
				{
					SetPropertyDependency(nameof(ButtonContent), value, nameof(NavigationViewModelBase.AsString));
				}

				outputViewModel = value;
				NotifyPropertyChanged();
			}
		}

		public string ButtonContent => getButtonContent();

		public Func<TOut, NavigationViewModelBase> OutputViewModelRequestHandler
		{
			private get => outputViewModelRequestHandler;

			set
			{
				outputViewModelRequestHandler = value;
				OutputViewModel = value?.Invoke(parentRow.Output);
			}
		}

		public NavigationViewModelBase outputViewModel { get; set; }

		public void NotifyButtonContentChanged()
		{
			NotifyPropertyChanged(nameof(ButtonContent));
		}

		public Button MakeOutputUi()
		{
			Button button = new() { FontSize = 14, Margin = new Thickness(40, 4, 4, 4), VerticalAlignment = VerticalAlignment.Center, MinWidth = 150, HorizontalAlignment = HorizontalAlignment.Left };
			button.Click += (s, e) => OnOutputButtonClick?.Invoke(OutputViewModel);

			Binding binding = new(nameof(ButtonContent)) { Source = this };
			_ = button.SetBinding(ContentControl.ContentProperty, binding);

			return button;
		}
	}
}
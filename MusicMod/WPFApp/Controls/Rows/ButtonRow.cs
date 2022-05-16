using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WPFApp.ViewModels;

namespace WPFApp.Controls.Rows
{
    internal abstract class ButtonRow<TOut, TRow> : Row<TOut, TRow>
        where TOut : class
        where TRow : ButtonRow<TOut, TRow>
    {
        private Func<TOut, NavigationViewModelBase> outputViewModelRequestHandler;

        protected ButtonRow(NavigationContext navigationContext, bool movable, bool removable = true) : base(movable, removable)
        {
            NavigationContext = navigationContext;
            OutputViewModelRequestHandler = navigationContext.GetViewModel;
            OnOutputButtonClick += (viewModel) => navigationContext.GoInto(viewModel);
            OnSetOutput += (output) =>
            {
                OutputViewModel = OutputViewModelRequestHandler?.Invoke(output);
            };
        }

        public event Action<NavigationViewModelBase> OnOutputButtonClick;

        public abstract string ButtonContent { get; }

        public NavigationViewModelBase OutputViewModel
        {
            get => outputViewModel;

            private set
            {
                var oldValue = OutputViewModel;

                RemovePropertyDependency(nameof(ButtonContent), oldValue, nameof(NavigationViewModelBase.AsString));

                if (oldValue is ItemViewModelBase oldItemViewModel)
                {
                    oldItemViewModel.OnItemChanged -= RefreshOutputUi;
                }

                if (value is ItemViewModelBase newItemViewModel)
                {
                    newItemViewModel.OnItemChanged += RefreshOutputUi;
                }

                if (value is RowViewModelBase rowViewModel)
                {
                    rowViewModel.RowManager.Parent = this;
                }

                if (value is not null)
                {
                    SetPropertyDependency(nameof(ButtonContent), value, nameof(NavigationViewModelBase.AsString));
                }

                outputViewModel = value;
                NotifyPropertyChanged();
            }
        }

        public Func<TOut, NavigationViewModelBase> OutputViewModelRequestHandler
        {
            private get => outputViewModelRequestHandler;

            set
            {
                outputViewModelRequestHandler = value;
                OutputViewModel = value?.Invoke(Output);
            }
        }

        protected NavigationContext NavigationContext { get; }

        protected virtual NavigationViewModelBase outputViewModel { get; set; }

        protected override UIElement MakeOutputUi()
        {
            Button button = new() { FontSize = 14, Margin = new Thickness(40, 4, 4, 4), VerticalAlignment = VerticalAlignment.Center, MinWidth = 150, HorizontalAlignment = HorizontalAlignment.Left };
            button.Click += (s, e) => OnOutputButtonClick?.Invoke(OutputViewModel);

            Binding binding = new(nameof(ButtonContent)) { Source = this };
            _ = button.SetBinding(ContentControl.ContentProperty, binding);

            return button;
        }
    }
}
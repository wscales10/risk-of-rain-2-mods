using Rules.RuleTypes.Mutable;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WPFApp.Controls.Wrappers;
using WPFApp.ViewModels;

namespace WPFApp.Controls.Rows
{
	internal abstract class RuleRow<TRow> : Row<Rule, TRow>, IRuleRow
		where TRow : RuleRow<TRow>
	{
		private Func<Rule, NavigationViewModelBase> outputViewModelRequestHandler;

		private IRuleRow parent;

		private NavigationViewModelBase outputViewModel;

		protected RuleRow(Rule output, bool movable, bool removable = true)
			: base(output, movable, removable)
		{
			SetPropertyDependency(nameof(ButtonContent), nameof(Output), nameof(Label), nameof(OutputViewModel));
			SetPropertyDependency(nameof(AllChildren), nameof(OutputViewModel));
		}

		public event Action<NavigationViewModelBase> OnOutputButtonClick;

		public virtual string Label => null;

		public IRuleRow Parent
		{
			get => parent;

			set => SetProperty(ref parent, value);
		}

		public override Rule Output
		{
			get => base.Output;

			protected set
			{
				base.Output = value;
				OutputViewModel = OutputViewModelRequestHandler?.Invoke(value);
			}
		}

		public Func<Rule, NavigationViewModelBase> OutputViewModelRequestHandler
		{
			private get => outputViewModelRequestHandler;

			set
			{
				outputViewModelRequestHandler = value;
				OutputViewModel = value?.Invoke(Output);
			}
		}

		public NavigationViewModelBase OutputViewModel
		{
			get => outputViewModel;

			private set
			{
				RemovePropertyDependency(nameof(ButtonContent), outputViewModel, nameof(NavigationViewModelBase.AsString));

				if (outputViewModel is ItemViewModelBase oldItemViewModel)
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

				if(value is not null)
                {
					SetPropertyDependency(nameof(ButtonContent), value, nameof(NavigationViewModelBase.AsString));
				}
				
				SetProperty(ref outputViewModel, value);
			}
		}

        public string ButtonContent => OutputViewModel?.AsString ?? (string.IsNullOrEmpty(Label) ? Output?.ToString() : Label);

        protected override ReadOnlyObservableCollection<IRow> AllChildren => (OutputViewModel as RowViewModelBase)?.RowManager.Rows;

		public override SaveResult TrySaveChanges() => new(Output is not null);

		public override string ToString() => Label ?? base.ToString();

		protected override UIElement MakeOutputUi()
		{
			if (Output is null)
			{
				ComboBox comboBox = new()
				{
					FontSize = 14,
					Margin = new Thickness(40, 4, 4, 4),
					VerticalAlignment = VerticalAlignment.Center,
					MinWidth = 150,
					HorizontalAlignment = HorizontalAlignment.Left
				};
				HelperMethods.MakeRulesComboBox(comboBox);
				comboBox.SelectionChanged += (s, e) => Output = Rule.Create((Type)comboBox.SelectedItem);
				return comboBox;
			}

			Button button = new() { FontSize = 14, Margin = new Thickness(40, 4, 4, 4), VerticalAlignment = VerticalAlignment.Center, MinWidth = 150, HorizontalAlignment = HorizontalAlignment.Left };
			button.Click += (s, e) => OnOutputButtonClick?.Invoke(OutputViewModel);

			Binding binding = new(nameof(ButtonContent)) { Source = this };
			_ = button.SetBinding(ContentControl.ContentProperty, binding);

			return button;
		}

		// TODO: it would be nice if this used the actual item names for buckets and skipped the "Bucket()" stuff
		protected override void RefreshOutputUi(UIElement ui, Rule output)
		{
			if (output is not null)
			{
				NotifyPropertyChanged(nameof(ButtonContent));
			}
		}
	}
}
using Rules.RuleTypes.Mutable;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace WPFApp.Controls.Rows
{
	internal abstract class RuleRow<TRow> : Row<Rule, TRow>, IRuleRow
		where TRow : RuleRow<TRow>
	{
		private ReadOnlyObservableCollection<IRow> children;

		private IRuleRow parent;

		protected RuleRow(Rule output, bool movable, bool removable = true)
			: base(output, movable, removable) => SetPropertyDependency(nameof(ButtonContent), nameof(Output), nameof(Label));

		public event Func<Rule, ControlBase> OnOutputControlRequested;

		public event Action<ControlBase> OnOutputButtonClick;

		public virtual string Label => null;

		public IRuleRow Parent
		{
			get => parent;

			set
			{
				parent = value;
				NotifyPropertyChanged();
			}
		}

		public override Rule Output
		{
			get => base.Output;

			protected set
			{
				base.Output = value;
				AllChildren = (OnOutputControlRequested?.Invoke(Output) as IRowControl)?.RowManager.Rows;
			}
		}

		public override ReadOnlyObservableCollection<IRow> AllChildren
		{
			protected get => children;

			set
			{
				children = value;
				NotifyPropertyChanged();
			}
		}

		public string ButtonContent => string.IsNullOrEmpty(Label) ? Output?.ToString() : Label;

		public override string ToString() => Label ?? base.ToString();

		protected override UIElement MakeOutputUi()
		{
			if (Output is null)
			{
				ComboBox comboBox = new() { FontSize = 14, Margin = new Thickness(40, 4, 4, 4), VerticalAlignment = VerticalAlignment.Center, MinWidth = 150, HorizontalAlignment = HorizontalAlignment.Left };
				HelperMethods.MakeRulesComboBox(comboBox);
				comboBox.SelectionChanged += (s, e) => Output = Rule.Create((Type)comboBox.SelectedItem);
				return comboBox;
			}

			Button button = new() { FontSize = 14, Margin = new Thickness(40, 4, 4, 4), VerticalAlignment = VerticalAlignment.Center, MinWidth = 150, HorizontalAlignment = HorizontalAlignment.Left };
			button.Click += (s, e) =>
			{
				var control = OnOutputControlRequested.Invoke(Output) as ItemControlBase;
				var rowControl = control as IRowControl;

				if (rowControl is not null)
				{
					rowControl.RowManager.Parent = this;
				}

				OnOutputButtonClick?.Invoke(control);

				if (control is not null)
				{
					control.OnItemChanged += RefreshOutputUi;
				}
			};

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
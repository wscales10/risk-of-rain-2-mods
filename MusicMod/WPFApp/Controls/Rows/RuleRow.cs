using Rules.RuleTypes.Mutable;
using System;
using System.Windows;
using System.Windows.Controls;

namespace WPFApp.Controls.Rows
{
	internal abstract class RuleRow<TRow> : Row<Rule, TRow>
		where TRow : RuleRow<TRow>
	{
		protected RuleRow(Rule output, bool movable, bool removable = true)
			: base(output, movable, removable)
		{
		}

		public event Func<Rule, ControlBase> OnOutputButtonClick;

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
				ControlBase control = OnOutputButtonClick?.Invoke(Output);

				if (control is not null)
				{
					control.OnItemChanged += RefreshOutputUi;
				}
			};

			return button;
		}

		// TODO: it would be nice if this used the actual item names for buckets and skipped the "Bucket()" stuff
		protected override void RefreshOutputUi(UIElement ui, Rule output)
		{
			if (output is not null)
			{
				((Button)ui).Content = output.ToString();
			}
		}
	}
}
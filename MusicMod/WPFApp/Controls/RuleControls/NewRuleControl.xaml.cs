using Rules.RuleTypes.Mutable;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace WPFApp.Controls.RuleControls
{
	/// <summary>
	/// Interaction logic for NewRuleControl.xaml
	/// </summary>
	public partial class NewRuleControl : UserControl
	{
		public NewRuleControl()
		{
			InitializeComponent();
			HelperMethods.MakeRulesComboBox(newRuleTypeComboBox, true);
		}

		public event Action<RuleBase> OnAddRule;

		public event Func<(Type, Type)> TypesRequested;

		public string ButtonText
		{
			get => (string)AddRuleButton.Content;
			set => AddRuleButton.Content = value;
		}

		private void AddRuleButton_Click(object sender, RoutedEventArgs e)
		{
			var (tContext, tOut) = TypesRequested();
			RuleBase rule = (RuleBase)typeof(Rule<,>).MakeGenericType(tContext, tOut).GetMethod("Create", BindingFlags.Static).Invoke(null, new[] { ((Type)newRuleTypeComboBox.SelectedItem).MakeGenericType(tContext, tOut) });

			if (rule is not null)
			{
				OnAddRule?.Invoke(rule);
			}
		}
	}
}
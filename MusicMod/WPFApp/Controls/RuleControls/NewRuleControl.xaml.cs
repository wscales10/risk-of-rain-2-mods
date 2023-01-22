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

		public event Func<(Type, Type)?> TypesRequested;

		public string ButtonText
		{
			get => (string)AddRuleButton.Content;
			set => AddRuleButton.Content = value;
		}

		private void AddRuleButton_Click(object sender, RoutedEventArgs e)
		{
			var types = TypesRequested();

			if (types is null)
			{
				return;
			}

			var (tContext, tOut) = types.Value;
			MethodInfo createMethodInfo = typeof(Rule<,>).MakeGenericType(tContext, tOut).GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
			RuleBase rule = (RuleBase)createMethodInfo.Invoke(null, (object[])(new[] { ((Type)newRuleTypeComboBox.SelectedItem) }));

			if (rule is not null)
			{
				OnAddRule?.Invoke(rule);
			}
		}
	}
}
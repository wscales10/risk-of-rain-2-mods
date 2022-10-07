using MyRoR2;
using Rules.RuleTypes.Mutable;
using Spotify.Commands;
using System;
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

        public event Action<Rule> OnAddRule;

        public string ButtonText
        {
            get => (string)AddRuleButton.Content;
            set => AddRuleButton.Content = value;
        }

        private void AddRuleButton_Click(object sender, RoutedEventArgs e)
        {
            var rule = Rule<Context, ICommandList>.Create((Type)newRuleTypeComboBox.SelectedItem);

            if (rule is not null)
            {
                OnAddRule?.Invoke(rule);
            }
        }
    }
}
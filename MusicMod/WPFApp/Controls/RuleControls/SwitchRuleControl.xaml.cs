using Patterns;
using Patterns.Patterns;
using Rules.RuleTypes.Mutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Utils;
using WPFApp.Controls.GridManagers;
using WPFApp.Controls.Rows;
using WPFApp.Controls.Wrappers;

namespace WPFApp.Controls.RuleControls
{
	/// <summary>
	/// Interaction logic for SwitchRuleControl.xaml
	/// </summary>
	public partial class SwitchRuleControl : RuleControlBase
	{
		private readonly RowManager<SwitchRow> rowManager;

		public SwitchRuleControl(StaticSwitchRule rule, NavigationContext ctx) : base(ctx)
		{
			Item = rule;
			InitializeComponent();
			propertyComboBox.ItemsSource = Info.ContextProperties;

			if (rule.PropertyInfo is not null)
			{
				propertyComboBox.SelectedItem = propertyComboBox.Items.Cast<PropertyInfo>().Single(p => p.Name == rule.PropertyInfo.Name);
			}

			propertyComboBox.SelectionChanged += (s, e) => Item.PropertyInfo = e.AddedItems.Cast<PropertyInfo>().Single();

			rowManager = new(casesGrid, AddDefaultButton);
			rowButtonsControl.BindTo(rowManager);
			rowManager.OnItemAdded += (row, _) => row.OnOutputButtonClick += NavigationContext.GoInto;
			rowManager.BindTo(Item.Cases, AddCase, r => ((CaseRow)r).Case, r => Item.DefaultRule = r?.Output);

			if (rule.DefaultRule is not null)
			{
				AddDefault(rule.DefaultRule);
			}

			Children = MappedObservableCollection.Create(rowManager.Items, row => (row.Label, ctx.GetControl(row.Output)));
		}

		public override StaticSwitchRule Item { get; }

		public override ReadOnlyObservableCollection<(string, RuleControlBase)> Children { get; }

		protected override SaveResult ShouldAllowExit()
		{
			// TODO: should consider whether patterns implement IPattern<selected property type>
			return new SaveResult(propertyComboBox.SelectedItem is not null) && rowManager.TrySaveChanges();
		}

		private CaseRow AddCase(Case<IPattern> c = null)
		{
			if (c is null)
			{
				c = new Case<IPattern>(null);
			}

			return rowManager.Add(new CaseRow(c, Item.PropertyInfo.Type, NavigationContext));
		}

		private void AddCaseButton_Click(object sender, RoutedEventArgs e) => AddCase();

		private void AddDefault(Rule rule = null) => _ = rowManager.AddDefault(new DefaultRow(rule, Item.PropertyInfo));

		private void AddDefaultButton_Click(object sender, RoutedEventArgs e) => AddDefault();
	}
}
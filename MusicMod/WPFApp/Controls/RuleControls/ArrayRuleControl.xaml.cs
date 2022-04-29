using Rules.RuleTypes.Mutable;
using WPFApp.Controls.GridManagers;
using WPFApp.Controls.Rows;
using Utils;

namespace WPFApp.Controls.RuleControls
{
	/// <summary>
	/// Interaction logic for ArrayRuleControl.xaml
	/// </summary>
	public partial class ArrayRuleControl : RuleControlBase
	{
		private readonly RowManager<ArrayRow> rowManager;

		public ArrayRuleControl(ArrayRule rule, NavigationContext ctx) : base(ctx)
		{
			Item = rule;
			InitializeComponent();
			newRuleControl.ButtonText = "Add Rule";
			newRuleControl.OnAddRule += r => AddRule(r);

			rowManager = new(rulesGrid);
			rowManager.OnItemAdded += (row, _) => row.OnOutputButtonClick += NavigationContext.GoInto;
			rowManager.BindTo(Item.Rules, AddRule, r => r.Output);

			rowButtonsControl.BindTo(rowManager);
			Children = MappedObservableCollection.Create(MappedObservableCollection.Create(rowManager.Items, row => ctx.GetControl(row.Output).Children));
		}

		public override ArrayRule Item { get; }

		public override MappedObservableCollection<(string, RuleControlBase)> Children { get; }

		private ArrayRow AddRule(Rule rule) => rowManager.Add(new ArrayRow(rule));
	}
}
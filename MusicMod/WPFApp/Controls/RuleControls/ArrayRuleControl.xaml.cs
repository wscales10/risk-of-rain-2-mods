using Rules.RuleTypes.Mutable;
using WPFApp.Controls.GridManagers;
using WPFApp.Controls.Rows;

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
			rowManager.OnRowAdded += (row, _) => row.OnOutputButtonClick += NavigationContext.GoInto;
			rowManager.BindTo(Item.Rules, AddRule, r => r.Output);
		}

		public override ArrayRule Item { get; }

		private ArrayRow AddRule(Rule rule) => new(rule, rowManager.Add);
	}
}
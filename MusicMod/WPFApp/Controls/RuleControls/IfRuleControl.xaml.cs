using MyRoR2;
using Patterns;
using Rules.RuleTypes.Mutable;
using System.Windows;
using WPFApp.Controls.GridManagers;
using WPFApp.Controls.Rows;

namespace WPFApp.Controls.RuleControls
{
	/// <summary>
	/// Interaction logic for IfRuleControl.xaml
	/// </summary>
	public partial class IfRuleControl : RuleControlBase
	{
		private readonly RowManager<IfRow> rowManager;

		public IfRuleControl(IfRule rule, NavigationContext ctx) : base(ctx)
		{
			Item = rule;
			InitializeComponent();

			rowManager = new(rulesGrid, AddElseButton);
			rowManager.OnRowRemoved += (_, _) => rule.ElseRule = null;
			rowManager.OnRowAdded += (row, _) => row.OnOutputButtonClick += NavigationContext.GoInto;

			thenRow = new(rule.Pattern, rule.ThenRule, rowManager.Add, NavigationContext);
			thenRow.OnSetOutput += (rule) => Item.ThenRule = rule;

			if (rule.ElseRule is not null)
			{
				AddElse(rule.ElseRule);
			}
		}

		private readonly ThenRow thenRow;

		public override IfRule Item { get; }

		private void AddElse(Rule rule = null)
		{
			var elseRow = new ElseRow(rule, rowManager.AddDefault);
			elseRow.OnSetOutput += (rule) => Item.ElseRule = rule;
		}

		private void AddElseButton_Click(object sender, RoutedEventArgs e) => AddElse();

		protected override bool ShouldAllowExit()
		{
			if (thenRow.PatternPickerWrapper.TryGetValue(out IPattern<Context> pattern))
			{
				Item.Pattern = pattern;
				return true;
			}

			return false;
		}
	}
}

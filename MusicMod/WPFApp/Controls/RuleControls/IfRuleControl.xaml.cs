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

		private readonly ThenRow thenRow;

		public IfRuleControl(IfRule rule, NavigationContext ctx) : base(ctx)
		{
			Item = rule;
			InitializeComponent();

			rowManager = new(rulesGrid, AddElseButton);
			rowButtonsControl.BindTo(rowManager);
			rowManager.OnItemRemoved += (_, _) => rule.ElseRule = null;
			rowManager.OnItemAdded += (row, _) => row.OnOutputButtonClick += NavigationContext.GoInto;

			thenRow = rowManager.Add(new ThenRow(rule.Pattern, rule.ThenRule, NavigationContext));
			thenRow.OnSetOutput += (rule) => Item.ThenRule = rule;

			if (rule.ElseRule is not null)
			{
				AddElse(rule.ElseRule);
			}
		}

		public override IfRule Item { get; }

		protected override bool ShouldAllowExit()
		{
			if (thenRow.PatternPickerWrapper.TryGetValue(out IPattern<Context> pattern))
			{
				Item.Pattern = pattern;
				return true;
			}

			return false;
		}

		private void AddElse(Rule rule = null)
		{
			ElseRow elseRow = new(rule);
			rowManager.AddDefault(elseRow);
			elseRow.OnSetOutput += (rule) => Item.ElseRule = rule;
		}

		private void AddElseButton_Click(object sender, RoutedEventArgs e) => AddElse();
	}
}
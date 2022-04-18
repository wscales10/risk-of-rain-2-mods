using Patterns;
using Rules.RuleTypes.Mutable;
using System;
using System.Windows.Controls;
using WPFApp.Controls.RuleControls;

namespace WPFApp.Controls.Rows
{
	internal class CaseRow : SwitchRow
	{
		public CaseRow(Case<IPattern> c, Type valueType, NodeGetter<SwitchRow> nodeGetter, NavigationContext navigationContext) : base(c.Output, nodeGetter, true)
		{
			LeftElement = new(c, valueType, navigationContext);
			PropagateUiChange(null, LeftElement);
			Case = c;
			OnSetOutput += (rule) => Case.Output = rule;
		}

		public Case<IPattern> Case { get; set; }

		public override CaseControl LeftElement { get; }

		public override bool TrySaveChanges() => LeftElement.TrySaveChanges();
	}

	internal class DefaultRow : SwitchRow
	{
		public DefaultRow(Rule rule, NodeGetter<SwitchRow> nodeGetter) : base(rule, nodeGetter, false) => ((TextBlock)LeftElement).Text = "Default";
	}

	internal abstract class SwitchRow : RuleRow<SwitchRow>
	{
		protected SwitchRow(Rule output, NodeGetter<SwitchRow> nodeGetter, bool movable)
			: base(output, nodeGetter, movable)
		{
		}
	}
}
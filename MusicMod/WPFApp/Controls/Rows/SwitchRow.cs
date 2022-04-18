using Patterns;
using Rules.RuleTypes.Mutable;
using System;
using System.Windows.Controls;
using WPFApp.Controls.RuleControls;

namespace WPFApp.Controls.Rows
{
	internal class CaseRow : SwitchRow
	{
		public CaseRow(Case<IPattern> c, Type valueType, IndexGetter<SwitchRow> indexGetter, NavigationContext navigationContext) : base(c.Output, indexGetter, true)
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
		public DefaultRow(Rule rule, IndexGetter<SwitchRow> indexGetter) : base(rule, indexGetter, false) => ((TextBlock)LeftElement).Text = "Default";
	}

	internal abstract class SwitchRow : RuleRow<SwitchRow>
	{
		protected SwitchRow(Rule output, IndexGetter<SwitchRow> indexGetter, bool movable)
			: base(output, indexGetter, movable)
		{
		}
	}
}
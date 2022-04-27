using Patterns;
using Rules.RuleTypes.Mutable;
using System;
using System.Windows.Controls;
using WPFApp.Controls.RuleControls;
using WPFApp.Controls.Wrappers;

namespace WPFApp.Controls.Rows
{
	internal class CaseRow : SwitchRow
	{
		public CaseRow(Case<IPattern> c, Type valueType, NavigationContext navigationContext) : base(c.Output, true)
		{
			LeftElement = new(c, valueType, navigationContext);
			PropagateUiChange(null, LeftElement);
			Case = c;
			OnSetOutput += (rule) => Case.Output = rule;
		}

		public Case<IPattern> Case { get; set; }

		public override CaseControl LeftElement { get; }

		public override SaveResult TrySaveChanges() => LeftElement.TrySaveChanges();
	}

	internal class DefaultRow : SwitchRow
	{
		public DefaultRow(Rule rule) : base(rule, false) => ((TextBlock)LeftElement).Text = "Default";
	}

	internal abstract class SwitchRow : RuleRow<SwitchRow>
	{
		protected SwitchRow(Rule output, bool movable) : base(output, movable)
		{
		}
	}
}
using Patterns;
using Patterns.Patterns;
using Rules.RuleTypes.Mutable;
using System;
using System.Windows.Controls;
using WPFApp.Controls.RuleControls;
using WPFApp.Controls.Wrappers;

namespace WPFApp.Controls.Rows
{
	internal class CaseRow : SwitchRow
	{
		private Case<IPattern> @case;

		public CaseRow(Case<IPattern> c, Type valueType, NavigationContext navigationContext) : base(c.Output, true)
		{
			SetPropertyDependency(nameof(Label), nameof(Case));
			LeftElement = new(c, valueType, navigationContext);
			PropagateUiChange(null, LeftElement);
			Case = c;
			OnSetOutput += (rule) => Case.Output = rule;
		}

		public Case<IPattern> Case
		{
			get => @case;

			set
			{
				@case = value;
				NotifyPropertyChanged();
			}
		}

		public override CaseControl LeftElement { get; }

		public override string Label => Case?.ToString();

		public override SaveResult TrySaveChanges() => LeftElement.TrySaveChanges();
	}

	internal class DefaultRow : SwitchRow
	{
		private readonly PropertyInfo propertyInfo;

		public DefaultRow(Rule rule, PropertyInfo propertyInfo) : base(rule, false)
		{
			((TextBlock)LeftElement).Text = "Default";
			this.propertyInfo = propertyInfo;
		}

		public override string Label => $"Other {propertyInfo}";
	}

	internal abstract class SwitchRow : RuleRow<SwitchRow>
	{
		protected SwitchRow(Rule output, bool movable) : base(output, movable)
		{
		}
	}
}
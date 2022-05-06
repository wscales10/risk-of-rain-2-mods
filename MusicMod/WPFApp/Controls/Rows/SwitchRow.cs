using Patterns;
using Patterns.Patterns;
using Rules.RuleTypes.Mutable;
using System;
using System.Windows.Controls;
using WPFApp.Controls.RuleControls;
using WPFApp.Controls.Wrappers;
using WPFApp.ViewModels;
using System.ComponentModel;

namespace WPFApp.Controls.Rows
{
	internal class CaseRow : SwitchRow
	{
		private readonly CaseViewModel caseViewModel;

		private Case<IPattern> @case;

		public CaseRow(Case<IPattern> c, Type valueType, NavigationContext navigationContext) : base(c.Output, true)
		{
			SetPropertyDependency(nameof(Label), nameof(Case));
			LeftElement.DataContext = caseViewModel = new CaseViewModel(c, valueType, navigationContext);
			caseViewModel.PropertyChanged += CaseViewModel_PropertyChanged;
			PropagateUiChange(null, LeftElement);
			Case = c;
			OnSetOutput += (rule) => Case.Output = rule;
		}

		public Case<IPattern> Case
		{
			get => @case;

			set => SetProperty(ref @case, value);
		}

		public override CaseControl LeftElement { get; } = new();

		public override string Label => Case?.ToString();

		public override SaveResult TrySaveChanges() => base.TrySaveChanges() & caseViewModel.TrySaveChanges();

		private void CaseViewModel_PropertyChanged(object _, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(CaseViewModel.CaseName))
			{
				NotifyPropertyChanged(nameof(Label));
			}
		}
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
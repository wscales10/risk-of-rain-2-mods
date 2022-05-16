using Patterns;
using Patterns.Patterns;
using Rules.RuleTypes.Mutable;
using System;
using System.Windows.Controls;
using WPFApp.Controls.RuleControls;
using WPFApp.ViewModels;
using System.Windows;
using WPFApp.Controls.Wrappers.SaveResults;

namespace WPFApp.Controls.Rows
{
    internal class CaseRow : SwitchRow
    {
        private readonly CaseViewModel caseViewModel;

        private readonly CaseControl caseControl = new();

        private Case<IPattern> @case;

        public CaseRow(Case<IPattern> c, Type valueType, NavigationContext navigationContext) : base(navigationContext, true)
        {
            SetPropertyDependency(nameof(Label), nameof(Case));
            SetPropertyDependency(nameof(Label), caseViewModel, nameof(CaseViewModel.CaseName));
            caseControl.DataContext = caseViewModel = new CaseViewModel(c, valueType, navigationContext);
            Case = c;
            OnSetOutput += (rule) => Case.Output = rule;
            Output = c.Output;
        }

        public Case<IPattern> Case
        {
            get => @case;

            set => SetProperty(ref @case, value);
        }

        public override UIElement LeftElement => caseControl;

        public override string Label => Case?.ToString();

        protected override SaveResult trySaveChanges() => base.trySaveChanges() & caseViewModel.TrySaveChanges();
    }

    internal class DefaultRow : SwitchRow
    {
        private readonly PropertyInfo propertyInfo;

        public DefaultRow(NavigationContext navigationContext, PropertyInfo propertyInfo) : base(navigationContext, false)
        {
            ((TextBlock)LeftElement).Text = "Default";
            this.propertyInfo = propertyInfo;
        }

        public override string Label => $"Other {propertyInfo}";
    }

    internal abstract class SwitchRow : RuleRow<SwitchRow>
    {
        protected SwitchRow(NavigationContext navigationContext, bool movable) : base(navigationContext, movable)
        {
        }
    }
}
using Patterns;
using Patterns.Patterns;
using Rules.RuleTypes.Mutable;
using System;
using System.Windows.Controls;
using WPFApp.ViewModels;
using System.Windows;
using WPFApp.Controls.RuleControls;
using WPFApp.SaveResults;

namespace WPFApp.Rows
{
    internal class CaseRow : SwitchRow
    {
        private readonly CaseViewModel caseViewModel;

        private readonly CaseControl caseControl;

        public CaseRow(Case<IPattern> c, PropertyWrapper<Type> valueType, NavigationContext navigationContext) : base(navigationContext, true)
        {
            Case = c;

            SetPropertyDependency(nameof(Label), nameof(Case));
            caseViewModel = new(c, valueType, navigationContext);
            caseControl = new() { DataContext = caseViewModel };
            SetPropertyDependency(nameof(Label), caseViewModel, nameof(CaseViewModel.CaseName));

            OnSetOutput += (rule) => Case.Output = rule;
            Output = c.Output;
        }

        public Case<IPattern> Case { get; }

        public override UIElement LeftElement => caseControl;

        public override string Label => Case?.ToString();

        protected override SaveResult trySaveChanges() => base.trySaveChanges() & !caseViewModel.HasErrors;
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
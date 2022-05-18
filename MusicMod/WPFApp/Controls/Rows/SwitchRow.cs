using Patterns;
using Patterns.Patterns;
using Rules.RuleTypes.Mutable;
using System;
using System.Windows.Controls;
using WPFApp.ViewModels;
using WPFApp.Controls.Wrappers.SaveResults;
using WPFApp.Controls.Wrappers;
using System.Windows;

namespace WPFApp.Controls.Rows
{
    internal class CaseRow : SwitchRow
    {
        private readonly CaseWrapper caseWrapper;

        public CaseRow(Case<IPattern> c, PropertyWrapper<Type> valueType, NavigationContext navigationContext) : base(navigationContext, true)
        {
            Case = c;

            SetPropertyDependency(nameof(Label), nameof(Case));
            caseWrapper = new(new(c, valueType, navigationContext));
            SetPropertyDependency(nameof(Label), caseWrapper.ViewModel, nameof(CaseViewModel.CaseName));

            OnSetOutput += (rule) => Case.Output = rule;
            Output = c.Output;
        }

        public Case<IPattern> Case { get; }

        public override UIElement LeftElement => caseWrapper.UIElement;

        public override string Label => Case?.ToString();

        protected override SaveResult trySaveChanges() => base.trySaveChanges() & caseWrapper.TryGetValue(true);
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
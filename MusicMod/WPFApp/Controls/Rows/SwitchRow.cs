using Patterns.Patterns;
using System;
using System.Windows.Controls;
using WPFApp.ViewModels;
using WPFApp.Controls.Wrappers.SaveResults;
using WPFApp.Controls.Wrappers;
using System.Windows;
using Case = Rules.RuleTypes.Mutable.Case;

namespace WPFApp.Controls.Rows
{
    internal class CaseRow : SwitchRow
    {
        private readonly CaseWrapper caseWrapper;

        private readonly PropertyWrapper<Type> valueType;

        public CaseRow(Case c, PropertyWrapper<Type> valueType, NavigationContext navigationContext) : base(navigationContext, true)
        {
            Case = c;
            this.valueType = valueType;
            SetPropertyDependency(nameof(Label), nameof(Case));
            caseWrapper = new(new(c, valueType, navigationContext));
            SetPropertyDependency(nameof(Label), caseWrapper.ViewModel, nameof(CaseViewModel.CaseName));

            OnSetOutput += (rule) => Case.Output = rule;
            Output = c.Output;
        }

        public Case Case { get; }

        public override UIElement LeftElement => caseWrapper.UIElement;

        public override string Label => Case?.ToString();

        protected override CaseRow deepClone() => new(Case.DeepClone(Info.PatternParser), valueType, NavigationContext);

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

        protected override DefaultRow deepClone() => new(NavigationContext, propertyInfo);
    }

    internal abstract class SwitchRow : RuleRow<SwitchRow>
    {
        protected SwitchRow(NavigationContext navigationContext, bool movable) : base(navigationContext, movable)
        {
        }
    }
}
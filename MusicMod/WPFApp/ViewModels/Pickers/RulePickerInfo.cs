using Rules.RuleTypes.Mutable;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WPFApp.ViewModels.Pickers
{
    internal class RulePickerInfo : TypeWrapperPickerInfo
    {
        public RulePickerInfo(NavigationContext navigationContext) : base(navigationContext ?? throw new ArgumentNullException(nameof(navigationContext)))
        {
        }

        public override object CreateItem(TypeWrapper selectedType)
        {
            var rule = Rule.Create(selectedType.Type);
            var viewModel = NavigationContext.GetViewModel(rule);
            return viewModel;
        }

        public override IEnumerable<TypeWrapper> GetItems() => Info.SupportedRuleTypes.Select(t => new TypeWrapper(t));
    }
}
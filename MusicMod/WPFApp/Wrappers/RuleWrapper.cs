using Rules.RuleTypes.Mutable;
using System;
using System.Windows;
using System.Windows.Controls;
using WPFApp.ViewModels.Pickers;

namespace WPFApp.Wrappers
{
    internal class RuleWrapper : SinglePickerWrapper<Rule>
    {
        private readonly Func<Button> buttonGetter;

        public RuleWrapper(NavigationContext navigationContext, Func<Button> buttonGetter) : base(new RulePickerInfo(navigationContext, buttonGetter))
        {
            this.buttonGetter = buttonGetter;
            UIElement.Alignment = HorizontalAlignment.Left;
            UIElement.FontSize = 14;
            UIElement.aligner.Margin = new Thickness(40, 4, 4, 4);
        }

        public Rule GetValueBypassValidation()
        {
            object obj = null;
            UIElement?.ViewModel?.ValueWrapper?.ForceGetValue(out obj);
            return (Rule)obj;
        }

        protected override bool Validate(Rule value) => value is not null;

        protected override void setValue(Rule value)
        {
            var valueWrapper = value is null ? null : new ItemButtonWrapper<Rule>(buttonGetter());
            valueWrapper?.SetValue(value);
            UIElement.ViewModel.HandleSelection(valueWrapper);
        }
    }
}
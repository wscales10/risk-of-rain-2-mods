using MyRoR2;
using Rules.RuleTypes.Mutable;
using System;
using System.Windows;
using System.Windows.Controls;
using WPFApp.Controls.Pickers;

namespace WPFApp.Controls.Wrappers
{
    internal class RuleWrapper : SinglePickerWrapper<Rule<Context>>
    {
        private readonly Func<Button> buttonGetter;

        public RuleWrapper(NavigationContext navigationContext, Func<Button> buttonGetter) : base(new RulePickerInfo(navigationContext, buttonGetter))
        {
            this.buttonGetter = buttonGetter;
            UIElement.Alignment = HorizontalAlignment.Left;
            UIElement.FontSize = 14;
            UIElement.aligner.Margin = new Thickness(40, 4, 4, 4);
        }

        public Rule<Context> GetValueBypassValidation()
        {
            object obj = null;
            UIElement?.ViewModel?.ValueWrapper?.ForceGetValue(out obj);
            return (Rule<Context>)obj;
        }

        protected override bool Validate(Rule<Context> value) => value is not null;

        protected override void setValue(Rule<Context> value)
        {
            var valueWrapper = value is null ? null : new ItemButtonWrapper<Rule<Context>>(buttonGetter());
            valueWrapper?.SetValue(value);
            UIElement.ViewModel.HandleSelection(valueWrapper);
        }
    }
}
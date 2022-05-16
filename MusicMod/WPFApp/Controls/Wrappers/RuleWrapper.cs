using System;
using System.Windows;
using System.Windows.Controls;
using WPFApp.Controls.Pickers;
using WPFApp.ViewModels;

namespace WPFApp.Controls.Wrappers
{
    internal class RuleWrapper : SinglePickerWrapper<NavigationViewModelBase>
    {
        private readonly Func<Button> buttonGetter;

        public RuleWrapper(NavigationContext navigationContext, Func<Button> buttonGetter) : base(new RulePickerInfo(navigationContext, buttonGetter))
        {
            this.buttonGetter = buttonGetter;
            UIElement.Alignment = HorizontalAlignment.Left;
            UIElement.FontSize = 14;
            UIElement.aligner.Margin = new Thickness(40, 4, 4, 4);
        }

        public NavigationViewModelBase GetValueBypassValidation()
        {
            object obj = null;
            UIElement?.ViewModel?.ValueWrapper?.ForceGetValue(out obj);
            return (NavigationViewModelBase)obj;
        }

        protected override void setValue(NavigationViewModelBase value)
        {
            ItemButtonWrapper valueWrapper = value is null ? null : new ItemButtonWrapper(buttonGetter());
            valueWrapper?.SetValue(value);
            UIElement.ViewModel.HandleSelection(valueWrapper);
        }
    }
}
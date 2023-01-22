using Rules.RuleTypes.Mutable;
using System;
using System.Windows;
using System.Windows.Controls;
using WPFApp.Controls.Pickers;

namespace WPFApp.Controls.Wrappers
{
	internal class RuleWrapper<TContext, TOut> : SinglePickerWrapper<Rule<TContext, TOut>>
	{
		public RuleWrapper(NavigationContext navigationContext, Func<Button> buttonGetter) : base(new RulePickerInfo<TContext, TOut>(navigationContext, buttonGetter))
		{
			UIElement.Alignment = HorizontalAlignment.Left;
			UIElement.FontSize = 14;
			UIElement.aligner.Margin = new Thickness(40, 4, 4, 4);
		}

		public Rule<TContext, TOut> GetValueBypassValidation()
		{
			object obj = null;
			UIElement?.ViewModel?.ValueWrapper?.ForceGetValue(out obj);
			return (Rule<TContext, TOut>)obj;
		}

		protected override bool Validate(Rule<TContext, TOut> value) => value is not null;

		protected override void setValue(Rule<TContext, TOut> value)
		{
			var valueWrapper = value is null ? null : CreateWrapper(value);
			// valueWrapper?.SetValue(value);
			UIElement.ViewModel.HandleSelection(valueWrapper);
		}
	}
}
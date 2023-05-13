using Patterns.Patterns;
using System;
using System.Windows.Controls;
using WPFApp.ViewModels;
using WPFApp.Controls.Wrappers.SaveResults;
using WPFApp.Controls.Wrappers;
using System.Windows;
using Rules.RuleTypes.Mutable;

namespace WPFApp.Controls.Rows
{
	internal class CaseRow<TContext, TOut> : SwitchRow<TContext, TOut>
	{
		private readonly CaseWrapper<TContext, TOut> caseWrapper;

		private readonly PropertyWrapper<Type> valueType;

		public CaseRow(RuleCase<TContext, TOut> c, PropertyWrapper<Type> valueType, NavigationContext navigationContext) : base(navigationContext, true)
		{
			Case = c;
			this.valueType = valueType;
			SetPropertyDependency(nameof(Label), nameof(Case));
			caseWrapper = new(new(c, valueType, navigationContext));
			SetPropertyDependency(nameof(Label), caseWrapper.ViewModel, nameof(CaseViewModel<TContext, TOut>.CaseName));

			OnSetOutput += (rule) => Case.Output = rule;
			Output = c.Output;
		}

		public RuleCase<TContext, TOut> Case { get; }

		public override UIElement LeftElement => caseWrapper.UIElement;

		public override string Label => Case?.ToString();

		protected override CaseRow<TContext, TOut> deepClone() => new(Case.DeepClone(Info.GetRuleParser<TContext, TOut>()), valueType, NavigationContext);

		protected override SaveResult trySaveChanges() => base.trySaveChanges() & caseWrapper.TryGetValue(true);
	}

	internal class DefaultRow<TContext, TOut> : SwitchRow<TContext, TOut>
	{
		private readonly PropertyInfo propertyInfo;

		public DefaultRow(NavigationContext navigationContext, PropertyInfo propertyInfo) : base(navigationContext, false)
		{
			((TextBlock)LeftElement).Text = "Default";
			this.propertyInfo = propertyInfo;
		}

		public override string Label => propertyInfo?.Name == "this" ? "Other" : $"Other {propertyInfo}";

		protected override DefaultRow<TContext, TOut> deepClone() => new(NavigationContext, propertyInfo);
	}

	internal abstract class SwitchRow<TContext, TOut> : RuleRow<SwitchRow<TContext, TOut>, TContext, TOut>
	{
		protected SwitchRow(NavigationContext navigationContext, bool movable) : base(navigationContext, movable)
		{
		}
	}
}
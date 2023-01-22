using Rules.RuleTypes.Mutable;
using System.Collections.Generic;
using WPFApp.Controls.GridManagers;
using WPFApp.Controls.Rows;

namespace WPFApp.ViewModels
{
	internal class ArrayRuleViewModel<TContext, TOut> : RuleViewModelBase<ArrayRule<TContext, TOut>>
	{
		public ArrayRuleViewModel(ArrayRule<TContext, TOut> arrayRule, NavigationContext navigationContext) : base(arrayRule, navigationContext)
		{
			ExtraCommands = new[]
			{
				new ButtonContext { Label = "Add Rule", Command = new ButtonCommand(_ => AddRule()) }
			};

			TypedRowManager.BindTo(Item.Rules, AddRule, r => r.Output);
		}

		public override string Title => "First match from:";

		public override IEnumerable<ButtonContext> ExtraCommands { get; }

		protected override RowManager<ArrayRow<TContext, TOut>> TypedRowManager { get; } = new();

		private ArrayRow<TContext, TOut> AddRule(Rule<TContext, TOut> rule = null) => TypedRowManager.Add(new(NavigationContext) { Output = rule });
	}
}
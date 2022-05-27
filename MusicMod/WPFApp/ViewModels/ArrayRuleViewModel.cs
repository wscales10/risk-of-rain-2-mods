using Rules.RuleTypes.Mutable;
using System;
using System.Collections.Generic;
using WPFApp.Controls.GridManagers;
using WPFApp.Rows;

namespace WPFApp.ViewModels
{
    internal class ArrayRuleViewModel : RuleViewModelBase<ArrayRule>
    {
        public ArrayRuleViewModel(ArrayRule arrayRule, NavigationContext navigationContext) : base(arrayRule, navigationContext)
        {
            ExtraCommands = new[]
            {
                new ButtonContext { Label = "Add Rule", Command = new ButtonCommand(_ => AddRule()) }
            };

            TypedRowManager.BindTo(Item.Rules, AddRule, r => r.Output);
        }

        public override string Title => "First match from:";

        public override IEnumerable<ButtonContext> ExtraCommands { get; }

        protected override RowManager<ArrayRow> TypedRowManager { get; } = new();

        private ArrayRow AddRule(Rule rule = null) => TypedRowManager.Add(new ArrayRow(NavigationContext) { Output = rule });
    }
}
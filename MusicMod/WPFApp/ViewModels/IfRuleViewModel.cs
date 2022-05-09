using Rules.RuleTypes.Mutable;
using System;
using System.Collections.Generic;
using WPFApp.Controls.GridManagers;
using WPFApp.Controls.Rows;
using WPFApp.Controls.Wrappers;

namespace WPFApp.ViewModels
{
    internal class IfRuleViewModel : RuleViewModelBase<IfRule>
    {
        private readonly ThenRow thenRow;

        private ElseRow elseRow;

        public IfRuleViewModel(IfRule ifRule, NavigationContext navigationContext) : base(ifRule, navigationContext)
        {
            ExtraCommands = new[]
            {
                new ButtonContext
                {
                    Label = "Add Else",
                    Command = new ButtonCommand(_ => AddElse(), TypedRowManager, nameof(TypedRowManager.HasDefault), (hasDefault) => !(bool)hasDefault)
                }
            };

            TypedRowManager.OnItemRemoved += (_, _) =>
            {
                ifRule.ElseRule = null;
                elseRow = null;
            };
            TypedRowManager.BeforeItemAdded += AttachRowEventHandlers;
            thenRow = TypedRowManager.Add(new ThenRow(ifRule.Pattern, ifRule.ThenRule, NavigationContext));
            thenRow.OnSetOutput += (rule) => Item.ThenRule = rule;

            if (ifRule.ElseRule is not null)
            {
                AddElse(ifRule.ElseRule);
            }
        }

        public override IEnumerable<ButtonContext> ExtraCommands { get; }

        protected override RowManager<IfRow> TypedRowManager { get; } = new();

        protected override SaveResult ShouldAllowExit()
        {
            var result1 = thenRow.PickerWrapper.TryGetValue(true);

            if (result1.IsSuccess)
            {
                Item.Pattern = result1.Value;
            }

            SaveResult result2 = result1;

            if (elseRow is not null)
            {
                result2 &= elseRow.TrySaveChanges();
            }

            return result2;
        }

        private void AddElse(Rule rule = null)
        {
            elseRow = new(rule);
            _ = TypedRowManager.AddDefault(elseRow);
            elseRow.OnSetOutput += (rule) => Item.ElseRule = rule;
        }
    }
}
using MyRoR2;
using Rules.RuleTypes.Mutable;
using System.Collections.Generic;
using WPFApp.Controls.GridManagers;
using WPFApp.Controls.Rows;
using WPFApp.Controls.Wrappers.SaveResults;

namespace WPFApp.ViewModels
{
    internal class IfRuleViewModel : RuleViewModelBase<IfRule<Context>>
    {
        public IfRuleViewModel(IfRule<Context> ifRule, NavigationContext navigationContext) : base(ifRule, navigationContext)
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
            };
            ThenRow thenRow = TypedRowManager.Add(new ThenRow(ifRule.Pattern, NavigationContext) { Output = ifRule.ThenRule });
            thenRow.OnSetOutput += (rule) => Item.ThenRule = rule;
            thenRow.Saved += ThenRow_Saved;

            if (ifRule.ElseRule is not null)
            {
                AddElse(ifRule.ElseRule);
            }
        }

        public override string Title => "If:";

        public override IEnumerable<ButtonContext> ExtraCommands { get; }

        protected override RowManager<IfRow> TypedRowManager { get; } = new();

        private void ThenRow_Saved(IfRow sender, SaveResult result)
        {
            if (result.IsSuccess)
            {
                Item.Pattern = ((ThenRow)sender).PickerWrapper.TryGetValue(false).Value;
            }
        }

        private void AddElse(Rule<Context> rule = null)
        {
            ElseRow elseRow = new(NavigationContext) { Output = rule };
            _ = TypedRowManager.AddDefault(elseRow);
            elseRow.OnSetOutput += (rule) => Item.ElseRule = rule;
        }
    }
}
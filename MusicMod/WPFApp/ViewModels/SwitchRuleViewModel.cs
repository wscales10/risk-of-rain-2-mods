using Patterns;
using Patterns.Patterns;
using Rules.RuleTypes.Mutable;
using System.Collections.Generic;
using WPFApp.Controls.GridManagers;
using WPFApp.Controls.Rows;
using WPFApp.Controls.Wrappers;

namespace WPFApp.ViewModels
{
    internal class SwitchRuleViewModel : RuleViewModelBase<StaticSwitchRule>
    {
        public SwitchRuleViewModel(StaticSwitchRule switchRule, NavigationContext navigationContext) : base(switchRule, navigationContext)
        {
            ExtraCommands = new[]
            {
                new ButtonContext
                {
                    Label = "Add Case",
                    Command = new ButtonCommand(_ => AddCase(), this, nameof(PropertyInfo), propertyInfo => propertyInfo is not null)
                },
                new ButtonContext
                {
                    Label = "Add Default",
                    Command = new ButtonCommand(
                        _ => AddDefault(),
                        new BindingPredicate(TypedRowManager, nameof(TypedRowManager.HasDefault), (hasDefault) => !(bool)hasDefault),
                        new BindingPredicate(this, nameof(PropertyInfo), propertyInfo => propertyInfo is not null))
                }
            };

            TypedRowManager.BeforeItemAdded += AttachRowEventHandlers;
            TypedRowManager.BindTo(Item.Cases, AddCase, r => ((CaseRow)r).Case, r => Item.DefaultRule = r?.Output);

            if (Item.DefaultRule is not null)
            {
                AddDefault(Item.DefaultRule);
            }
        }

        public override string Title => "Switch on:";

        public PropertyInfo PropertyInfo
        {
            get => Item.PropertyInfo;

            set
            {
                Item.PropertyInfo = value;
                NotifyPropertyChanged();
            }
        }

        public override IEnumerable<ButtonContext> ExtraCommands { get; }

        protected override RowManager<SwitchRow> TypedRowManager { get; } = new();

        protected override SaveResult ShouldAllowExit()
        {
            // TODO: should consider whether patterns implement IPattern<selected property type>
            return new SaveResult(PropertyInfo is not null) && RowManager.TrySaveChanges();
        }

        private void AddDefault(Rule rule = null) => _ = TypedRowManager.AddDefault(new DefaultRow(rule, Item.PropertyInfo));

        private CaseRow AddCase(Case<IPattern> c = null)
        {
            if (c is null)
            {
                c = new Case<IPattern>(null);
            }

            return TypedRowManager.Add(new CaseRow(c, Item.PropertyInfo.Type, NavigationContext));
        }
    }
}
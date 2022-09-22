using MyRoR2;
using Patterns;
using Patterns.Patterns;
using Rules.RuleTypes.Mutable;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using WPFApp.Controls.GridManagers;
using WPFApp.Controls.Rows;
using WPFApp.Controls.Wrappers;
using WPFApp.Controls.Wrappers.SaveResults;
using Case = Rules.RuleTypes.Mutable.RuleCase<MyRoR2.Context>;

namespace WPFApp.ViewModels
{
    internal class SwitchRuleViewModel : RuleViewModelBase<StaticSwitchRule<Context>>
    {
        private readonly PropertyWrapper<Type> valueType = new();

        public SwitchRuleViewModel(StaticSwitchRule<Context> switchRule, NavigationContext navigationContext) : base(switchRule, navigationContext)
        {
            PropertyInfo.OnValidate += PropertyInfo_OnValidate;
            PropertyInfo.PropertyChanged += PropertyInfo_PropertyChanged;
            PropertyInfo.Value = Item.PropertyInfo;

            ExtraCommands = new[]
            {
                new ButtonContext
                {
                    Label = "Add Case",
                    Command = new ButtonCommand(_ => AddCase(), PropertyInfo, nameof(PropertyInfo.Value), propertyInfo => propertyInfo is not null)
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

            TypedRowManager.BindTo(Item.Cases, AddCase, r => ((CaseRow)r).Case, r => Item.DefaultRule = r?.Output);

            if (Item.DefaultRule is not null)
            {
                AddDefault(Item.DefaultRule);
            }
        }

        public override string Title => "Switch on:";

        public StaticMutableSaveResult<PropertyInfo> PropertyInfo { get; } = new();

        public override IEnumerable<ButtonContext> ExtraCommands { get; }

        protected override RowManager<SwitchRow> TypedRowManager { get; } = new();

        protected override SaveResult<StaticSwitchRule<Context>> ShouldAllowExit()
        {
            PropertyInfo.MaybeOutput(propertyInfo => Item.PropertyInfo = propertyInfo);
            return base.ShouldAllowExit() & PropertyInfo;
        }

        private void PropertyInfo_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PropertyInfo.Value))
            {
                valueType.Value = PropertyInfo.Value?.Type;
            }
        }

        private void PropertyInfo_OnValidate(object sender, MyValidationEventArgs2<PropertyInfo> e)
        {
            if (e.Value is null)
            {
                e.Invalidate();
            }
        }

        private void AddDefault(Rule<Context> rule = null) => _ = TypedRowManager.AddDefault(new DefaultRow(NavigationContext, Item.PropertyInfo) { Output = rule });

        private CaseRow AddCase(Case c = null)
        {
            if (c is null)
            {
                c = new Case(null);
            }

            return TypedRowManager.Add(new CaseRow(c, valueType, NavigationContext));
        }
    }
}
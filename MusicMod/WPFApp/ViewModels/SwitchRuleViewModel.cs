using Patterns.Patterns;
using Rules.RuleTypes.Mutable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using WPFApp.Controls.GridManagers;
using WPFApp.Controls.Rows;
using WPFApp.Controls.Wrappers;
using WPFApp.Controls.Wrappers.SaveResults;

namespace WPFApp.ViewModels
{
    internal class SwitchRuleViewModel<TContext, TOut> : RuleViewModelBase<StaticSwitchRule<TContext, TOut>>
    {
        private readonly PropertyWrapper<Type> valueType = new();

        public SwitchRuleViewModel(StaticSwitchRule<TContext, TOut> switchRule, NavigationContext navigationContext) : base(switchRule, navigationContext)
        {
            PropertyInfo.OnValidate += PropertyInfo_OnValidate;
            PropertyInfo.PropertyChanged += PropertyInfo_PropertyChanged;
            PropertyInfo.Value = Item.PropertyInfo;

            ExtraCommands = new[]
            {
                new ButtonContext
                {
                    Label = "Insert Case",
                    Command = new ButtonCommand(_ => InsertCase(), PropertyInfo, nameof(PropertyInfo.Value), propertyInfo => propertyInfo is not null)
                },
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

            TypedRowManager.BindTo(Item.Cases, InsertCase, r => ((CaseRow<TContext, TOut>)r).Case, r => Item.DefaultRule = r?.Output);

            if (Item.DefaultRule is not null)
            {
                AddDefault(Item.DefaultRule);
            }
        }

        public ReadOnlyCollection<PropertyInfo> ContextProperties => Info.ContextProperties[typeof(TContext)];

        public override string Title => "Switch on:";

        public StaticMutableSaveResult<PropertyInfo> PropertyInfo { get; } = new();

        public override IEnumerable<ButtonContext> ExtraCommands { get; }

        protected override RowManager<SwitchRow<TContext, TOut>> TypedRowManager { get; } = new();

        protected override SaveResult<StaticSwitchRule<TContext, TOut>> ShouldAllowExit()
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

        private void AddDefault(Rule<TContext, TOut> rule = null) => _ = TypedRowManager.AddDefault(new DefaultRow<TContext, TOut>(NavigationContext, Item.PropertyInfo) { Output = rule });

        private CaseRow<TContext, TOut> InsertCase(RuleCase<TContext, TOut> c = null)
        {
            if (c is null)
            {
                c = new(null);
            }

            return TypedRowManager.Add(new CaseRow<TContext, TOut>(c, valueType, NavigationContext));
        }

        private CaseRow<TContext, TOut> AddCase(RuleCase<TContext, TOut> c = null)
        {
            TypedRowManager.SelectNone();
            var output = InsertCase(c);
            TypedRowManager.SetSelection(output);
            return output;
        }
    }
}
using System;
using System.Collections.Generic;
using WPFApp.Controls.GridManagers;
using WPFApp.Controls.Rows;
using Patterns;
using Patterns.Patterns;
using WPFApp.Controls.Wrappers;
using Utils;

namespace WPFApp.ViewModels
{
    internal class ListPatternViewModel : PatternViewModelBase<IListPattern>
    {
        private readonly Type valueType;

        public ListPatternViewModel(IListPattern pattern, Type valueType, NavigationContext navigationContext) : base(pattern, navigationContext)
        {
            ExtraCommands = new[]
            {
                new ButtonContext
                {
                    Label = "Add Pattern",
                    Command = new ButtonCommand
                    (
                        _ => AddPattern(),
                        TypedRowManager,
                        nameof(TypedRowManager.HasDefault),
                        hasDefault => !(bool)hasDefault
                    )
                }
            };

            TypedRowManager.BindLooselyTo(Item.Children, AddPattern, valuegetter);
            this.valueType = valueType;
        }

        public override string Title
        {
            get
            {
                var type = Item.GetType();

                if (type.IsGenericType(typeof(AndPattern<>)))
                {
                    return "Match all of:";
                }
                else if (type.IsGenericType(typeof(OrPattern<>)))
                {
                    return "Match any of:";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public override IEnumerable<ButtonContext> ExtraCommands { get; }

        protected override RowManager<PatternRow> TypedRowManager { get; } = new();

        private static SaveResult<IPattern> valuegetter(PatternRow row)
        {
            SaveResult result = row.TrySaveChanges();
            return SaveResult.Create<IPattern>(result);
        }

        private PatternRow AddPattern(IPattern pattern = null) => TypedRowManager.Add(new(pattern, valueType, NavigationContext));
    }
}
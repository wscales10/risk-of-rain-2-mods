using MyRoR2;
using Patterns;
using System.Windows.Controls;
using WPFApp.Controls.Wrappers.PatternWrappers;
using System.Windows;
using WPFApp.Controls.Wrappers.SaveResults;
using Rules.RuleTypes.Mutable;

namespace WPFApp.Controls.Rows
{
    internal abstract class IfRow : RuleRow<IfRow>
    {
        protected IfRow(NavigationContext navigationContext, bool removable = true) : base(navigationContext, false, removable)
        {
        }
    }

    internal class ThenRow : IfRow
    {
        public ThenRow(IPattern<Context> pattern, NavigationContext navigationContext) : base(navigationContext, false)
        {
            PickerWrapper = new SinglePatternPickerWrapper<Context>(navigationContext);

            if (pattern is not null)
            {
                PickerWrapper.SetValue(pattern);
            }
        }

        public SinglePatternPickerWrapper<Context> PickerWrapper { get; }

        public override UIElement LeftElement => PickerWrapper.UIElement;

        public override string Label => PickerWrapper?.TryGetValue(false).Value?.ToString();

        protected override ThenRow deepClone()
        {
            PickerWrapper.ForceGetValue(out IPattern<Context> pattern);
            return new ThenRow(Info.PatternParser.Parse<Context>(pattern.ToXml()), NavigationContext);
        }

        protected override SaveResult<IPattern<Context>> trySaveChanges() => PickerWrapper.TryGetValue(true) & base.trySaveChanges();
    }

    internal class ElseRow : IfRow
    {
        public ElseRow(NavigationContext navigationContext) : base(navigationContext) => ((TextBlock)LeftElement).Text = "Else";

        public override string Label => "Otherwise";

        protected override ElseRow deepClone() => new(NavigationContext);
    }
}
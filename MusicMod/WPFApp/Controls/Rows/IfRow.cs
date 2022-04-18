using MyRoR2;
using Patterns;
using Rules.RuleTypes.Mutable;
using System.Windows.Controls;
using WPFApp.Controls.Wrappers.PatternWrappers;
using WPFApp.Controls.PatternControls;

namespace WPFApp.Controls.Rows
{
	internal abstract class IfRow : RuleRow<IfRow>
	{
		protected IfRow(Rule output, IndexGetter<IfRow> indexGetter, bool removable = true) : base(output, indexGetter, false, removable)
		{
		}
	}

	internal class ThenRow : IfRow
	{
		public ThenRow(IPattern<Context> pattern, Rule rule, IndexGetter<IfRow> indexGetter, NavigationContext navigationContext) : base(rule, indexGetter, false)
		{
			PatternPickerWrapper = new SinglePatternPickerWrapper<Context>(navigationContext);
			LeftElement = PatternPickerWrapper.UIElement;
			PropagateUiChange(null, LeftElement);

			if (pattern is not null)
			{
				PatternPickerWrapper.SetValue(pattern);
			}
		}

		public SinglePatternPickerWrapper<Context> PatternPickerWrapper { get; }

		public override SinglePatternPicker LeftElement { get; }

		public override bool TrySaveChanges() => PatternPickerWrapper.TryGetValue(out IPattern<Context> _);
	}

	internal class ElseRow : IfRow
	{
		public ElseRow(Rule rule, IndexGetter<IfRow> indexGetter) : base(rule, indexGetter) => ((TextBlock)LeftElement).Text = "Else";
	}
}
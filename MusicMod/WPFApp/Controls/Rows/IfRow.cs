using Patterns;
using System.Windows.Controls;
using WPFApp.Controls.Wrappers.PatternWrappers;
using System.Windows;
using WPFApp.Controls.Wrappers.SaveResults;

namespace WPFApp.Controls.Rows
{
	internal abstract class IfRow<TContext, TOut> : RuleRow<IfRow<TContext, TOut>, TContext, TOut>
	{
		protected IfRow(NavigationContext navigationContext, bool removable = true) : base(navigationContext, false, removable)
		{
		}
	}

	internal class ThenRow<TContext, TOut> : IfRow<TContext, TOut>
	{
		public ThenRow(IPattern<TContext> pattern, NavigationContext navigationContext) : base(navigationContext, false)
		{
			PickerWrapper = new SinglePatternPickerWrapper<TContext>(navigationContext);

			if (pattern is not null)
			{
				PickerWrapper.SetValue(pattern);
			}
		}

		public SinglePatternPickerWrapper<TContext> PickerWrapper { get; }

		public override UIElement LeftElement => PickerWrapper.UIElement;

		public override string Label => PickerWrapper?.TryGetValue(false).Value?.ToString();

		protected override ThenRow<TContext, TOut> deepClone()
		{
			PickerWrapper.ForceGetValue(out IPattern<TContext> pattern);
			return new ThenRow<TContext, TOut>(Info.PatternParser.Parse<TContext>(pattern.ToXml()), NavigationContext);
		}

		protected override SaveResult<IPattern<TContext>> trySaveChanges() => PickerWrapper.TryGetValue(true) & base.trySaveChanges();
	}

	internal class ElseRow<TContext, TOut> : IfRow<TContext, TOut>
	{
		public ElseRow(NavigationContext navigationContext) : base(navigationContext) => ((TextBlock)LeftElement).Text = "Else";

		public override string Label => "Otherwise";

		protected override ElseRow<TContext, TOut> deepClone() => new(NavigationContext);
	}
}
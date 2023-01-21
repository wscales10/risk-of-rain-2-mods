using Rules.RuleTypes.Mutable;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using WPFApp.Controls.Wrappers;
using WPFApp.Controls.Wrappers.SaveResults;
using WPFApp.ViewModels;

namespace WPFApp.Controls.Rows
{
	internal abstract class RuleRow<TRow, TContext, TOut> : ButtonRow<Rule<TContext, TOut>, TRow>, IRuleRow
		where TRow : RuleRow<TRow, TContext, TOut>
	{
		private RuleWrapper<TContext, TOut> ruleWrapper;

		private IRuleRow parent;

		protected RuleRow(NavigationContext navigationContext, bool movable, bool removable = true)
			: base(navigationContext, movable, removable)
		{
			SetPropertyDependency(nameof(ButtonContent), nameof(Output), nameof(Label), nameof(OutputViewModel));
			SetPropertyDependency(nameof(AllChildren), nameof(OutputViewModel));
			RuleWrapper.ValueSet += _ => NotifyPropertyChanged(nameof(Output));
		}

		public override string ButtonContent => OutputViewModel?.AsString ?? (string.IsNullOrEmpty(Label) ? Output?.ToString() : Label);

		public virtual string Label => null;

		public IRuleRow Parent
		{
			get => parent;

			set => SetProperty(ref parent, value);
		}

		public override Rule<TContext, TOut> Output
		{
			get => RuleWrapper.GetValueBypassValidation();

			set => RuleWrapper.SetValue(value);
		}

		RuleBase IRuleRow.Output => Output;

		protected override ReadOnlyObservableCollection<IRow> AllChildren => (OutputViewModel as RowViewModelBase)?.RowManager.Rows;

		private RuleWrapper<TContext, TOut> RuleWrapper => ruleWrapper ??= new(NavigationContext, () => (Button)base.MakeOutputUi());

		public override string ToString() => Label ?? base.ToString();

		protected sealed override Rule<TContext, TOut> CloneOutput() => Info.GetRuleParser<TContext, TOut>().Parse(Output.ToXml());

		protected override SaveResult trySaveChanges() => base.trySaveChanges() & RuleWrapper.TryGetValue(true);

		protected override UIElement MakeOutputUi() => RuleWrapper.UIElement;

		protected override void RefreshOutputUi(UIElement ui, Rule<TContext, TOut> output)
		{
			if (output is not null)
			{
				NotifyPropertyChanged(nameof(ButtonContent));
			}
		}
	}
}
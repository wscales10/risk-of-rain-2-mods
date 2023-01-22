using Rules.RuleTypes.Mutable;
using System.Collections.ObjectModel;
using System.Windows;
using WPFApp.Controls.Wrappers;
using WPFApp.Controls.Wrappers.SaveResults;
using WPFApp.ViewModels;

namespace WPFApp.Controls.Rows
{
	internal abstract class RuleRow<TRow, TContext, TOut> : Row<Rule<TContext, TOut>, TRow>, IRuleRow
		where TRow : RuleRow<TRow, TContext, TOut>
	{
		private RuleWrapper<TContext, TOut> ruleWrapper;

		private IRuleRow parent;

		protected RuleRow(NavigationContext navigationContext, bool movable, bool removable = true)
			: base(movable, removable)
		{
			NavigationContext = navigationContext;

			ButtonOutputUi = new(
				navigationContext,
				RefreshOutputUi,
				this,
				new(
					() => ButtonOutputUi.OutputViewModel?.AsString ?? (string.IsNullOrEmpty(Label) ? Output?.ToString() : Label),
					new DependencyInformation(this, nameof(Output), nameof(Label))));
			SetPropertyDependency(nameof(AllChildren), ButtonOutputUi, nameof(ButtonOutputUi.OutputViewModel));
			RuleWrapper.ValueSet += _ => NotifyPropertyChanged(nameof(Output));
		}

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

		public NavigationViewModelBase OutputViewModel => ButtonOutputUi?.OutputViewModel;

		protected ButtonOutputUi<Rule<TContext, TOut>> ButtonOutputUi { get; }

		protected NavigationContext NavigationContext { get; }

		protected override ReadOnlyObservableCollection<IRow> AllChildren => (ButtonOutputUi.OutputViewModel as RowViewModelBase)?.RowManager.Rows;

		private RuleWrapper<TContext, TOut> RuleWrapper => ruleWrapper ??= new(NavigationContext, () => ButtonOutputUi.MakeOutputUi());

		public override string ToString() => Label ?? base.ToString();

		protected sealed override Rule<TContext, TOut> CloneOutput() => Info.GetRuleParser<TContext, TOut>().Parse(Output.ToXml());

		protected override SaveResult trySaveChanges() => base.trySaveChanges() & RuleWrapper.TryGetValue(true);

		protected override UIElement MakeOutputUi() => RuleWrapper.UIElement;

		protected override void RefreshOutputUi(UIElement ui, Rule<TContext, TOut> output)
		{
			if (output is not null)
			{
				ButtonOutputUi.NotifyButtonContentChanged();
			}
		}
	}
}
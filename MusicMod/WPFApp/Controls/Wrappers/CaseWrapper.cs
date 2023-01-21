using Patterns;
using WPFApp.Controls.RuleControls;
using WPFApp.Controls.Wrappers.SaveResults;
using WPFApp.ViewModels;
using System.Linq;
using System;
using System.Windows.Media;
using Rules.RuleTypes.Mutable;

namespace WPFApp.Controls.Wrappers
{
	internal class CaseWrapper<TContext, TOut> : ReadableControlWrapper<RuleCase<TContext, TOut>, CaseControl>
	{
		public CaseWrapper(CaseViewModel<TContext, TOut> viewModel)
		{
			UIElement.DataContext = ViewModel = viewModel;
			SetPropertyDependency(nameof(ValueString), ViewModel, nameof(CaseViewModel<TContext, TOut>.CaseName));
		}

		public override string ValueString => ViewModel.CaseName;

		public override CaseControl UIElement { get; } = new();

		public CaseViewModel<TContext, TOut> ViewModel { get; }

		private Type constructedInterface => typeof(IPattern<>).MakeGenericType(ViewModel.ValueType);

		protected override SaveResult<RuleCase<TContext, TOut>> tryGetValue(GetValueRequest request)
		{
			SaveResult success = ViewModel.PickerViewModel.ValueContainerManager.TrySaveChanges();
			SaveResult<object> result = ViewModel.WherePatternWrapper.TryGetObject(true);

			if (result.IsSuccess)
			{
				ViewModel.Case.WherePattern = (IPattern<TContext>)result.Value;
			}

			// check that type matches
			bool status = ViewModel.Case.Arr.All(constructedInterface.IsInstanceOfType);
			success &= new SaveResult(status, () => StopHighlighting());

			return new(success & result, ViewModel.Case);
		}

		protected override void setStatus(bool status)
		{
			int i = 0;
			foreach (var container in ViewModel.PickerViewModel.ValueContainerManager.Items)
			{
				Outline(container, status || constructedInterface.IsInstanceOfType(ViewModel.Case.Arr[i]), Brushes.Transparent);
				i++;
			}
		}
	}
}
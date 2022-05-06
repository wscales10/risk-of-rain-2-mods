using MyRoR2;
using Patterns;
using Rules.RuleTypes.Mutable;
using System;
using System.Windows;
using WPFApp.Controls.PatternControls;
using WPFApp.Controls.Wrappers;
using WPFApp.Controls.Wrappers.PatternWrappers;

namespace WPFApp.ViewModels
{
	public class CaseViewModel : ViewModelBase
	{
		private readonly NavigationContext navigationContext;

		private OptionalPatternPickerWrapper<Context> wherePatternWrapper;

		private bool wherePatternError;

		public CaseViewModel(Case<IPattern> c, Type valueType, NavigationContext navigationContext)
		{
			SetPropertyDependency(nameof(WherePatternStatus), nameof(WherePatternError), nameof(WherePatternWrapper));
			NavigationContext = navigationContext;
			WherePatternWrapper_StatusSet(null);
			WherePatternWrapper.StatusSet += WherePatternWrapper_StatusSet;
			WherePatternWrapper.SetValue(c.WherePattern);
			this.navigationContext = navigationContext;
			PatternPicker = new(valueType, navigationContext);
			PatternPicker.VerticalAlignment = VerticalAlignment.Center;
			PatternPicker.PatternContainerManager.BindLooselyTo(c.Arr, AddPattern, (PatternContainer c) => SaveResult.Create<IPattern>(c.PatternWrapper.TryGetObject(true)));
			Case = c;
		}

		public MultiPatternPicker PatternPicker { get; }

		public Case<IPattern> Case { get; }

		public string CaseName
		{
			get => Case.Name;

			set
			{
				Case.Name = value?.Length == 0 ? null : value;
				NotifyPropertyChanged();
			}
		}

		public bool? WherePatternStatus => HasWherePattern ? (!WherePatternError) : null;

		public OptionalPatternPickerWrapper<Context> WherePatternWrapper
		{
			get => wherePatternWrapper;
			private set => SetProperty(ref wherePatternWrapper, value);
		}

		public bool WherePatternError
		{
			get => wherePatternError;
			private set => SetProperty(ref wherePatternError, value);
		}

		internal NavigationContext NavigationContext
		{
			set => WherePatternWrapper = new OptionalPatternPickerWrapper<Context>(value);
		}

		internal bool HasWherePattern => WherePatternWrapper.UIElement.patternContainer.PatternWrapper is not null;

		public SaveResult TrySaveChanges()
		{
			SaveResult success = PatternPicker.PatternContainerManager.TrySaveChanges();
			SaveResult<object> result = WherePatternWrapper.TryGetObject(true);

			if (result.IsSuccess)
			{
				Case.WherePattern = (IPattern<Context>)result.Value;
			}

			return success & result;
		}

		private void WherePatternWrapper_StatusSet(bool? status) => WherePatternError = !(status ?? true);

		private PatternContainer AddPattern(IPattern pattern) => PatternPicker.AddPatternWrapper(PatternWrapper.Create(pattern, navigationContext));
	}
}
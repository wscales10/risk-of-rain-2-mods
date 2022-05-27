using MyRoR2;
using Patterns;
using Rules.RuleTypes.Mutable;
using System;
using WPFApp.Wrappers.PatternWrappers;
using WPFApp.SaveResults;
using WPFApp.ViewModels.Pickers;

namespace WPFApp.ViewModels
{
    public class CaseViewModel : ViewModelBase
    {
        private readonly PropertyWrapper<Type> valueType;

        private NavigationContext navigationContext;

        private OptionalPickerWrapper<Context> wherePatternWrapper;

        private bool wherePatternError;

        public CaseViewModel(Case<IPattern> c, PropertyWrapper<Type> valueType, NavigationContext navigationContext)
        {
            DefinePropertyValidation(nameof())

                SaveResult success = ViewModel.PickerViewModel.ValueWrapperManager.TrySaveChanges();
            SaveResult<object> result = ViewModel.WherePatternWrapper.TryGetObject(true);

            if (result.IsSuccess)
            {
                ViewModel.Case.WherePattern = (IPattern<Context>)result.Value;
            }

            // check that type matches
            bool status = ViewModel.Case.Arr.All(constructedInterface.IsInstanceOfType);
            success &= new SaveResult(status, () => StopHighlighting());

            return new(success & result, Case);

            Case = c;
            this.valueType = valueType;
            NavigationContext = navigationContext;

            PickerViewModel = new(new PatternPickerInfo(valueType, navigationContext));
            PickerViewModel.ValueWrapperManager.BindLooselyTo(c.Arr, AddPattern, wrapper => SaveResult.Create<IPattern>(wrapper.TryGetObject(true)));

            SetPropertyDependency(nameof(WherePatternStatus), nameof(WherePatternError), nameof(WherePatternWrapper));
            WherePatternWrapper_StatusSet(null);
            WherePatternWrapper.StatusSet += WherePatternWrapper_StatusSet;
            WherePatternWrapper.SetValue(c.WherePattern);
        }

        public Type ValueType => valueType;

        public MultiPickerViewModel PickerViewModel { get; }

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

        public OptionalPickerWrapper<Context> WherePatternWrapper
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
            get => navigationContext;

            set
            {
                navigationContext = value;
                WherePatternWrapper = new OptionalPickerWrapper<Context>(value);
            }
        }

        internal bool HasWherePattern => WherePatternWrapper.UIElement.ViewModel.ValueWrapper is not null;

        private void WherePatternWrapper_StatusSet(bool? status) => WherePatternError = !(status ?? true);

        private IReadableControlWrapper AddPattern(IPattern pattern) => PickerViewModel.AddWrapper(PatternWrapper.Create(pattern, navigationContext));
    }
}
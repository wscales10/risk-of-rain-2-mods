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

        private OptionalPickerWrapper<Context> wherePatternWrapper;

        private bool wherePatternError;

        public CaseViewModel(Case<IPattern> c, Type valueType, NavigationContext navigationContext)
        {
            SetPropertyDependency(nameof(WherePatternStatus), nameof(WherePatternError), nameof(WherePatternWrapper));
            NavigationContext = navigationContext;
            WherePatternWrapper_StatusSet(null);
            WherePatternWrapper.StatusSet += WherePatternWrapper_StatusSet;
            WherePatternWrapper.SetValue(c.WherePattern);
            this.navigationContext = navigationContext;
            MultiPickerViewModel pickerViewModel = new(new PatternPickerInfo(valueType, navigationContext));
            Picker = new(pickerViewModel);
            Picker.VerticalAlignment = VerticalAlignment.Center;
            pickerViewModel.ValueContainerManager.BindLooselyTo(c.Arr, AddPattern, (ValueContainer c) => SaveResult.Create<IPattern>(c.ValueWrapper.TryGetObject(true)));
            Case = c;
        }

        public MultiPicker Picker { get; }

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
            set => WherePatternWrapper = new OptionalPickerWrapper<Context>(value);
        }

        internal bool HasWherePattern => WherePatternWrapper.UIElement.ViewModel.ValueWrapper is not null;

        public SaveResult TrySaveChanges()
        {
            SaveResult success = Picker.ViewModel.ValueContainerManager.TrySaveChanges();
            SaveResult<object> result = WherePatternWrapper.TryGetObject(true);

            if (result.IsSuccess)
            {
                Case.WherePattern = (IPattern<Context>)result.Value;
            }

            return success & result;
        }

        private void WherePatternWrapper_StatusSet(bool? status) => WherePatternError = !(status ?? true);

        private ValueContainer AddPattern(IPattern pattern) => Picker.ViewModel.AddWrapper(PatternWrapper.Create(pattern, navigationContext));
    }
}
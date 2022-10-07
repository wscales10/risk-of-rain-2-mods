using MyRoR2;
using Patterns;
using System;
using System.Windows;
using WPFApp.Controls;
using WPFApp.Controls.Pickers;
using WPFApp.Controls.Wrappers.PatternWrappers;
using WPFApp.Controls.Wrappers.SaveResults;
using Case = Rules.RuleTypes.Mutable.RuleCase<MyRoR2.Context, Spotify.Commands.ICommandList>;

namespace WPFApp.ViewModels
{
    public class CaseViewModel : ViewModelBase
    {
        private readonly PropertyWrapper<Type> valueType;

        private NavigationContext navigationContext;

        private OptionalPickerWrapper<Context> wherePatternWrapper;

        private bool wherePatternError;

        public CaseViewModel(Case c, PropertyWrapper<Type> valueType, NavigationContext navigationContext)
        {
            Case = c;
            this.valueType = valueType;
            NavigationContext = navigationContext;

            PickerViewModel = new(new PatternPickerInfo(valueType, navigationContext));
            MultiPicker picker = new() { ViewModel = PickerViewModel };
            picker.VerticalAlignment = VerticalAlignment.Center;
            PickerViewModel.ValueContainerManager.BindLooselyTo(c.Arr, AddPattern, container => SaveResult.Create<IPattern>(container.ValueWrapper.TryGetObject(true)));

            SetPropertyDependency(nameof(WherePatternStatus), nameof(WherePatternError), nameof(WherePatternWrapper));
            WherePatternWrapper_StatusSet(null);
            WherePatternWrapper.StatusSet += WherePatternWrapper_StatusSet;
            WherePatternWrapper.SetValue(c.WherePattern);
        }

        public Type ValueType => valueType;

        public MultiPickerViewModel PickerViewModel { get; }

        public Case Case { get; }

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

        private ValueContainer AddPattern(IPattern pattern) => PickerViewModel.AddWrapper(PatternWrapper.Create(pattern, navigationContext));
    }
}
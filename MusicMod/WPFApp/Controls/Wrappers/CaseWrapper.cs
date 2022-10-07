using MyRoR2;
using Patterns;
using WPFApp.Controls.RuleControls;
using WPFApp.Controls.Wrappers.SaveResults;
using WPFApp.ViewModels;
using System.Linq;
using System;
using System.Windows.Media;
using Case = Rules.RuleTypes.Mutable.RuleCase<MyRoR2.Context, Spotify.Commands.ICommandList>;

namespace WPFApp.Controls.Wrappers
{
    internal class CaseWrapper : ReadableControlWrapper<Case, CaseControl>
    {
        public CaseWrapper(CaseViewModel viewModel)
        {
            UIElement.DataContext = ViewModel = viewModel;
            SetPropertyDependency(nameof(ValueString), ViewModel, nameof(CaseViewModel.CaseName));
        }

        public override string ValueString => ViewModel.CaseName;

        public override CaseControl UIElement { get; } = new();

        public CaseViewModel ViewModel { get; }

        private Type constructedInterface => typeof(IPattern<>).MakeGenericType(ViewModel.ValueType);

        protected override SaveResult<Case> tryGetValue(GetValueRequest request)
        {
            SaveResult success = ViewModel.PickerViewModel.ValueContainerManager.TrySaveChanges();
            SaveResult<object> result = ViewModel.WherePatternWrapper.TryGetObject(true);

            if (result.IsSuccess)
            {
                ViewModel.Case.WherePattern = (IPattern<Context>)result.Value;
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
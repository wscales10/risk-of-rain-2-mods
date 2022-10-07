using MyRoR2;
using Rules.RuleTypes.Mutable;
using Spotify.Commands;
using System;
using System.Collections;
using System.Linq;
using System.Windows.Controls;
using WPFApp.Controls.Wrappers;

namespace WPFApp.Controls.Pickers
{
    internal class RulePickerInfo : IPickerInfo
    {
        private readonly Func<Button> buttonGetter;

        public RulePickerInfo(NavigationContext navigationContext, Func<Button> buttonGetter)
        {
            NavigationContext = navigationContext ?? throw new ArgumentNullException(nameof(navigationContext));
            this.buttonGetter = buttonGetter;
        }

        public string DisplayMemberPath => nameof(TypeWrapper.DisplayName);

        public string SelectedValuePath => nameof(TypeWrapper.Type);

        public NavigationContext NavigationContext { get; }

        public IControlWrapper CreateWrapper(object selectedInfo)
        {
            var output = new ItemButtonWrapper<Rule<Context, ICommandList>>(buttonGetter());
            output.SetValue(Rule<Context, ICommandList>.Create((Type)selectedInfo));
            return output;
        }

        public IEnumerable GetItems() => Info.SupportedRuleTypes.Select(t => new TypeWrapper(t));

        IReadableControlWrapper IPickerInfo.CreateWrapper(object selectedInfo) => CreateWrapper(selectedInfo);
    }
}
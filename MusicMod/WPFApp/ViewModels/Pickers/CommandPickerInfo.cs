using System.Collections.Generic;
using System.Linq;
using Utils.Reflection;

namespace WPFApp.ViewModels.Pickers
{
    internal class CommandPickerInfo : TypeWrapperPickerInfo
    {
        public CommandPickerInfo(NavigationContext navigationContext) : base(navigationContext)
        {
        }

        public override object CreateItem(TypeWrapper selectedType)
        {
            var command = selectedType.Type.Construct();
            return command; // TODO: maybe return formatstring here?
        }

        public override IEnumerable<TypeWrapper> GetItems() => Info.SupportedCommandTypes.Select(t => new TypeWrapper(t));
    }
}
using WPFApp.Properties;

namespace WPFApp.ViewModels
{
    public class ViewModelBase : NotifyDataErrorInfoBase
    {
        internal static Settings Settings => Settings.Default;
    }
}
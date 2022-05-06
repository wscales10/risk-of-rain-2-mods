using WPFApp.Properties;

namespace WPFApp.ViewModels
{
	public class ViewModelBase : NotifyPropertyChangedBase
	{
		internal static Settings Settings => Settings.Default;
	}
}
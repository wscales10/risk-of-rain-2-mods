using WPFApp.Controls.Wrappers.SaveResults;

namespace WPFApp.ViewModels
{
	internal interface INameableViewModel
	{
		string Name { get; set; }

		string NameWatermark { get; }

		string Title { get; }

		MutableSaveResultBase<string> NameResult { get; }

		INavigationContext NavigationContext { get; }
	}
}
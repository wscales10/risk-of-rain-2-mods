using System.ComponentModel;

namespace WPFApp.Views
{
	internal interface IView<TViewModel>
		where TViewModel : INotifyPropertyChanged
	{
		TViewModel ViewModel { get; }
	}
}
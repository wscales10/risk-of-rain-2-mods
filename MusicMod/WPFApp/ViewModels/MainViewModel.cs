using WPFApp.Controls;
using WPFApp.Modes;

namespace WPFApp.ViewModels
{
	public class MainViewModel : ViewModelBase
	{
		private ControlBase control;

		private bool hasContent;

		private bool isXmlControl;

		public MainViewModel(NavigationContext navigationContext) => NavigationContext = navigationContext;

		public NavigationContext NavigationContext { get; }

		public ControlBase Control
		{
			get => control;

			set
			{
				control = value;
				HasContent = value is not null;
				IsXmlControl = value is IXmlControl;
				NotifyPropertyChanged();
			}
		}

		public bool HasContent
		{
			get => hasContent;

			private set
			{
				hasContent = value;
				NotifyPropertyChanged();
			}
		}

		public bool IsXmlControl
		{
			get => isXmlControl;

			private set
			{
				isXmlControl = value;
				NotifyPropertyChanged();
			}
		}
	}
}
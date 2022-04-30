using System.Collections;
using System.Windows.Input;

namespace WPFApp.ViewModels
{
	public class RuleTreeViewModel : ViewModelBase
	{
		private bool isCollapsed;

		private IEnumerable itemsSource;

		private ICommand navigateTreeCommand;

		public IEnumerable ItemsSource
		{
			get => itemsSource;

			set
			{
				itemsSource = value;
				NotifyPropertyChanged();
			}
		}

		public ICommand NavigateTreeCommand
		{
			get => navigateTreeCommand;

			set
			{
				navigateTreeCommand = value;
				NotifyPropertyChanged();
			}
		}

		public bool IsCollapsed
		{
			get => isCollapsed;

			set
			{
				isCollapsed = value;
				NotifyPropertyChanged();
			}
		}
	}
}
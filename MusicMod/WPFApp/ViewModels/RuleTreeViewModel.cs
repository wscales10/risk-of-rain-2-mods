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

			set => SetProperty(ref itemsSource, value);
		}

		public ICommand NavigateTreeCommand
		{
			get => navigateTreeCommand;

			set => SetProperty(ref navigateTreeCommand, value);
		}

		public bool IsCollapsed
		{
			get => isCollapsed;

			set => SetProperty(ref isCollapsed, value);
		}
	}
}
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using WPFApp.Controls;
using WPFApp.Properties;
using WPFApp.ViewModels;
using System.ComponentModel;
using System.Windows.Input;

namespace WPFApp.Views
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainView : Window
	{
		private readonly NavigationContext navigationContext;

		public MainView(NavigationContext navigationContext)
		{
			DataContext = new MainViewModel(navigationContext);
			InitializeComponent();
			OpenLinksInAppCheckbox.IsChecked = Settings.Default.OpenLinksInApp;
			newRuleControl.ButtonText = "Create New Rule";
			newRuleControl.OnAddRule += (r) => navigationContext.GoInto(r);
			this.navigationContext = navigationContext;
		}

		public event Action OnGoBack;

		public event Action OnGoForward;

		public event Action<string> OnImportFile;

		public event Action<string> OnExportFile;

		public event Action OnReset;

		public event Func<bool> OnTryEnableAutosave;

		public event Func<bool> OnTryClose;

		public static string GetExportLocation() => TryGetExportLocation(out string fileName) ? fileName : null;

		public void UpdateNavigationButtons(int historyIndex, int reverseIndex)
		{
			BackButton.IsEnabled = historyIndex > 0;
			ForwardButton.IsEnabled = reverseIndex > 0;
		}

		public void Display(ControlBase control) => ControlContainer.Content = control;

		private static bool TryGetExportLocation(out string fileName)
		{
			SaveFileDialog dialog = new() { Filter = "XML Files (*.xml)|*.xml|All files (*.*)|*.*" };

			if (dialog.ShowDialog() == true)
			{
				fileName = dialog.FileName;
				return true;
			}
			else
			{
				fileName = null;
				return false;
			}
		}

		private void HomeButton_Click(object sender, RoutedEventArgs e) => navigationContext.GoHome();

		private void UpLevelButton_Click(object sender, RoutedEventArgs e) => navigationContext.GoUp();

		private void ImportButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dialog = new() { Filter = "XML Files (*.xml)|*.xml|All files (*.*)|*.*" };

			if (dialog.ShowDialog() == true)
			{
				OnImportFile?.Invoke(dialog.FileName);
			}
		}

		private void ExportButton_Click(object sender, RoutedEventArgs e)
		{
			if (TryGetExportLocation(out string fileName))
			{
				OnExportFile?.Invoke(fileName);
			}
		}

		private void BackButton_Click(object sender, RoutedEventArgs e) => OnGoBack?.Invoke();

		private void ForwardButton_Click(object sender, RoutedEventArgs e) => OnGoForward?.Invoke();

		private void Window_MouseDown(object sender, MouseButtonEventArgs e) => _ = masterGrid.Focus();

		private void NewButton_Click(object sender, RoutedEventArgs e)
		{
			if (MessageBox.Show("Are you sure you want to close this?", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
			{
				OnReset?.Invoke();
			}
		}

		private void Window_SizeChanged(object sender, SizeChangedEventArgs e) => masterGrid.Focus();

		private void Window_Deactivated(object sender, EventArgs e) => masterGrid.Focus();

		private void AutosaveCheckbox_Click(object sender, RoutedEventArgs e)
		{
			var checkbox = (CheckBox)sender;

			if (checkbox.IsChecked.Value)
			{
				if (OnTryEnableAutosave?.Invoke() != true)
				{
					checkbox.IsChecked = false;
				}
			}
			else
			{
				Settings.Default.Autosave = false;
			}
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			if (OnTryClose?.Invoke() == false)
			{
				e.Cancel = true;
			}
		}
	}
}
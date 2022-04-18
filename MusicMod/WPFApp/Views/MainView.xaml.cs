using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WPFApp.Controls;
using WPFApp.Properties;
using System.Windows.Controls.Primitives;

namespace WPFApp.Views
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainView : Window
	{
		private ControlBase control;

		public MainView(NavigationContext navigationContext)
		{
			InitializeComponent();
			Binding binding = new(nameof(navigationContext.IsOffline))
			{
				Source = navigationContext,
				Mode = BindingMode.TwoWay
			};

			_ = OfflineModeCheckbox.SetBinding(ToggleButton.IsCheckedProperty, binding);

			OfflineModeCheckbox.IsChecked = Settings.Default.OfflineMode;
			OpenLinksInAppCheckbox.IsChecked = Settings.Default.OpenLinksInApp;
			newRuleControl.ButtonText = "Create New Rule";
			newRuleControl.OnAddRule += (r) => navigationContext.GoInto(r);
		}

		public event Action OnGoHome;

		public event Action OnGoBack;

		public event Action OnGoForward;

		public event Action OnGoUp;

		public event Action<string> OnImportFile;

		public event Action<string> OnExportFile;

		public event Action OnReset;

		public ControlBase Control
		{
			get => control;
			set
			{
				masterGrid.Children.Remove(control);
				control = value;

				if (control is null)
				{
					newRuleControl.Visibility = Visibility.Visible;
					NewButton.IsEnabled = false;
				}
				else
				{
					Grid.SetRow(control, 1);
					_ = masterGrid.Children.Add(control);
					newRuleControl.Visibility = Visibility.Hidden;
					NewButton.IsEnabled = true;
				}

				ExportButton.IsEnabled = control is IXmlControl;
			}
		}

		public void UpdateNavigationButtons(bool isHome, int historyIndex, int historyCount)
		{
			HomeButton.IsEnabled = !isHome;
			UpLevelButton.IsEnabled = !isHome;
			BackButton.IsEnabled = historyIndex > 0;
			ForwardButton.IsEnabled = historyIndex < historyCount;
		}

		private void HomeButton_Click(object sender, RoutedEventArgs e) => OnGoHome?.Invoke();

		private void UpLevelButton_Click(object sender, RoutedEventArgs e) => OnGoUp?.Invoke();

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
			SaveFileDialog dialog = new() { Filter = "XML Files (*.xml)|*.xml|All files (*.*)|*.*" };

			if (dialog.ShowDialog() == true)
			{
				OnExportFile?.Invoke(dialog.FileName);
			}
		}

		private void BackButton_Click(object sender, RoutedEventArgs e) => OnGoBack?.Invoke();

		private void ForwardButton_Click(object sender, RoutedEventArgs e) => OnGoForward?.Invoke();

		private void OpenLinksInAppCheckbox_Checked(object sender, RoutedEventArgs e)
		{
			Settings.Default.OpenLinksInApp = true;
			Settings.Default.Save();
		}

		private void OpenLinksInAppCheckbox_Unchecked(object sender, RoutedEventArgs e)
		{
			Settings.Default.OpenLinksInApp = false;
			Settings.Default.Save();
		}

		private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => _ = masterGrid.Focus();

		private void NewButton_Click(object sender, RoutedEventArgs e)
		{
			if (MessageBox.Show("Are you sure you want to close this?", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
			{
				OnReset?.Invoke();
			}
		}

		private void Window_SizeChanged(object sender, SizeChangedEventArgs e) => masterGrid.Focus();

		private void Window_Deactivated(object sender, EventArgs e) => masterGrid.Focus();
	}
}

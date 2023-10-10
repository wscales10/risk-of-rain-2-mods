using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using WPFApp.Properties;
using WPFApp.ViewModels;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Data;
using WPFApp.Controls.Rows;
using System.IO;

namespace WPFApp.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainView(MainViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
            OpenLinksInAppCheckbox.IsChecked = Settings.Default.OpenLinksInApp;
            newRuleControl.ButtonText = "Create New Rule";
            newRuleControl.OnAddRule += (r) => viewModel.NavigationContext.GoInto(r);
        }

        public event Func<bool> OnTryEnableAutosave;

        public event Func<bool> OnTryClose;

        public static FileInfo GetExportLocation() => TryGetExportLocation(out string fileName) ? new(fileName) : null;

        public void Display(NavigationViewModelBase control) => ControlContainer.Content = control;

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

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) => _ = masterGrid.Focus();

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

        private void CollectionViewSource_Filter(object sender, FilterEventArgs e) => e.Accepted = e.Item is IRuleRow;
    }
}
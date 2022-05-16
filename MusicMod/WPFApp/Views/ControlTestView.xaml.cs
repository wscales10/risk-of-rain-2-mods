using System.Windows;
using System.Windows.Controls;

namespace WPFApp.Views
{
    /// <summary>
    /// Interaction logic for ControlTestView.xaml
    /// </summary>
    public partial class ControlTestView : Window
    {
        public ControlTestView()
        {
            DataContext = new ViewModels.TestComboBoxViewModel();
            InitializeComponent();
        }
    }
}
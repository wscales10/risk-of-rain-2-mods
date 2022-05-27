using System.Windows.Controls;
using System.Windows.Input;

namespace WPFApp.Controls.Commands
{
    /// <summary>
    /// Interaction logic for PropertyString.xaml
    /// </summary>
    public partial class PropertyString : UserControl
    {
        public PropertyString() => InitializeComponent();

        private void Label_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) => FocusElement.Focus();
    }
}
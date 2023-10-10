using System;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Utils;

namespace WPFApp.Views
{
    /// <summary>
    /// Interaction logic for ClipboardWindow.xaml
    /// </summary>
    public partial class ClipboardWindow : Window
    {
        public ClipboardWindow(XmlClipboard clipboard)
        {
            (Clipboard = clipboard).OnSelect += Close;
            InitializeComponent();
            Loaded += ClipboardWindow_Loaded;
            Activated += ClipboardWindow_Activated;
            Closing += ClipboardWindow_Closing;
        }

        public XmlClipboard Clipboard { get => DataContext as XmlClipboard; set => DataContext = value; }

        private void ClipboardWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }

        private void ClipboardWindow_Activated(object sender, System.EventArgs e)
        {
            var str = System.Windows.Clipboard.GetText();
            Clipboard.SetExternalItem(str);
        }

        private void ClipboardWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Clipboard.Choose(null);
        }

        private void ListBoxItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Clipboard.PasteCommand.Execute(null);
        }
    }
}
using System.Windows;
using System.Windows.Controls;

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
		}

		public XmlClipboard Clipboard { get => DataContext as XmlClipboard; set => DataContext = value; }

		private void ClipboardWindow_Loaded(object sender, RoutedEventArgs e)
		{
			Clipboard.Choose(null);
		}
	}
}
using Spotify;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Utils;
using WPFApp.Properties;

namespace WPFApp.Controls
{
    public delegate Task<ConditionalValue<MusicItemInfo>> MusicItemInfoRequestHandler(SpotifyItem item);

    /// <summary>
    /// Interaction logic for SpotifyItemPicker.xaml
    /// </summary>
    public partial class SpotifyItemPicker : UserControl
    {
        private static readonly Regex regex = new(@"https?:\/\/open.spotify.com\/(?<itemType>.*?)\/(?<id>\w*)");

        public SpotifyItemPicker()
        {
            DataContext = ViewModel = new();
            InitializeComponent();
        }

        internal static event MusicItemInfoRequestHandler MusicItemInfoRequested;

        public static AsyncCache<SpotifyItem, MusicItemInfo> MusicItemDictionary { get; } = new(si => MusicItemInfoRequested?.Invoke(si));

        public SpotifyItemPickerViewModel ViewModel { get; }

        private static Uri GetUri(SpotifyItem item) => Settings.Default.OpenLinksInApp ? item.GetUri() : item.GetUrl();

        private void SpotifyItemPicker_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MusicItemInfo info = ((SpotifyItemPickerViewModel)DataContext).Info;
            if (info is not null)
            {
                Web.Goto(GetUri(info.PreviewItem));
            }
        }

        private void Border_PreviewDrop(object sender, DragEventArgs e)
        {
            object dataObject = e?.Data?.GetData(typeof(string));

            if (dataObject is null || dataObject is not string url)
            {
                this.Log("No data");
                return;
            }

            Match match = regex.Match(url);

            if (!match.Success)
            {
                this.Log("Unrecognised url format");
                return;
            }

            string itemType = match.Groups["itemType"]?.Value;
            string id = match.Groups["id"]?.Value;

            ViewModel.Item = new(itemType.AsEnum<SpotifyItemType>(true), id);
        }

        private void TextBlock_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var frameworkElement = (FrameworkElement)sender;
            var creator = (Creator)frameworkElement.DataContext;
            Web.Goto(GetUri(creator.SpotifyItem));
            e.Handled = true;
        }

        private void Border_MouseMove(object sender, MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Pressed && ViewModel.Item is SpotifyItem item)
            {
                // Package the data.
                DataObject data = new();
                data.SetData(item.GetUrl().AbsoluteUri);

                // Initiate the drag-and-drop operation.
                DragDrop.DoDragDrop(this, data, DragDropEffects.Copy);
            }
        }
    }
}
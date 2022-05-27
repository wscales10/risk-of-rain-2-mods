using Spotify;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Utils;
using WPFApp.Properties;

namespace WPFApp.Controls
{
    /// <summary>
    /// Interaction logic for SpotifyItemPicker.xaml
    /// </summary>
    public partial class SpotifyItemPicker : UserControl
    {
        public static readonly DependencyPropertyKey InfoPropertyKey = DependencyProperty.RegisterReadOnly(nameof(Info), typeof(MusicItemInfo), typeof(SpotifyItemPicker), new(null));

        public static readonly DependencyPropertyKey ImageSourcePropertyKey = DependencyProperty.RegisterReadOnly(nameof(ImageSource), typeof(ImageSource), typeof(SpotifyItemPicker), new(null));

        private static readonly Regex regex = new(@"https?:\/\/open.spotify.com\/(?<itemType>.*?)\/(?<id>\w*)");

        private CancellationTokenSource individualCancellationTokenSource = new();

        public SpotifyItemPicker()
        {
            DataContextChanged += SpotifyItemPicker_DataContextChanged;
            OnConnectionMade += RequestMusicItemInfo;

            SetPropertyDependency(nameof(ItemName), nameof(Item), nameof(Info));
            SetPropertyDependency(nameof(Type), nameof(Item), nameof(Info));
            SetPropertyDependency(nameof(HasCreators), nameof(Info));

            InitializeComponent();
        }

        private static event Action OnConnectionMade;

        public SpotifyItemPickerViewModel ViewModel { get; }

        public bool HasCreators => Info?.Creators?.Length > 0;

        public string Type
        {
            get
            {
                if (Info is null)
                {
                    return Item?.Type.ToString();
                }
                else
                {
                    return Info.MusicItem.Type.ToString();
                }
            }
        }

        public MusicItemInfo Info
        {
            get => (MusicItemInfo)GetValue(InfoPropertyKey.DependencyProperty);

            private set
            {
                SetValue(InfoPropertyKey.DependencyProperty, value);
                SetImageSource();
            }
        }

        public string ItemName
        {
            get
            {
                if (Item is null)
                {
                    return "Drop Spotify item here";
                }

                if (Info is null)
                {
                    return Item?.Id;
                }

                return Info.Name;
            }
        }

        public ImageSource ImageSource
        {
            get => (ImageSource)GetValue(ImageSourcePropertyKey.DependencyProperty);
            private set => SetValue(ImageSourcePropertyKey.DependencyProperty, value);
        }

        private SpotifyItem Item => DataContext as SpotifyItem;

        public static void Goto(SpotifyItem item)
        {
            Web.Goto(GetUri(item));
        }

        public void RequestMusicItemInfo()
        {
            individualCancellationTokenSource?.Cancel();
            individualCancellationTokenSource = new();
            _ = RequestMusicItemInfoAsync();
        }

        internal static void Refresh() => OnConnectionMade?.Invoke();

        private static Uri GetUri(SpotifyItem item) => Settings.Default.OpenLinksInApp ? item.GetUri() : item.GetUrl();

        private void SpotifyItemPicker_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ImageSource = null;
            RequestMusicItemInfo();
        }

        private async Task RequestMusicItemInfoAsync()
        {
            Info = await ResourceManagers.SpotifyInfo.RequestResource(Item, individualCancellationTokenSource.Token);
        }

        private void SetImageSource()
        {
            string imageUrl = Info?.Images.OrderBy(i => i.Width * i.Height).FirstOrDefault()?.Url;
            ImageSource = Images.BuildFromUri(imageUrl);
        }

        private void SpotifyItemPicker_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MusicItemInfo info = Info;
            if (info is not null)
            {
                Web.Goto(GetUri(info.PreviewItem));
            }
        }

        private void Border_PreviewDrop(object sender, DragEventArgs e)
        {
            object dataObject = e?.Data?.GetData(typeof(string));

            if (dataObject is not string url)
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

            DataContext = new SpotifyItem(itemType.AsEnum<SpotifyItemType>(true), id);
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
            if (e.LeftButton == MouseButtonState.Pressed && Item is SpotifyItem item)
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
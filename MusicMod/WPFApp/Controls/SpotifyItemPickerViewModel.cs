using Spotify;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Utils.Async;
using WPFApp.ViewModels;

namespace WPFApp.Controls
{
    public class SpotifyItemPickerViewModel : ViewModelBase
    {
        private static readonly CancellationTokenSource cancellationTokenSource = new();

        private static readonly TaskMachine taskMachine = new(cancellationTokenSource.Token);

        private MusicItemInfo info;

        private SpotifyItem? item;

        private ImageSource imageSource;

        public SpotifyItemPickerViewModel()
        {
            SetPropertyDependency(nameof(Name), nameof(Item), nameof(Info));
            SetPropertyDependency(nameof(Type), nameof(Item), nameof(Info));
            SetPropertyDependency(nameof(HasCreators), nameof(Info));
            OnConnectionMade += RequestMusicItemInfo;
        }

        private static event Action OnConnectionMade;

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

        public SpotifyItem? Item
        {
            get => item;

            set
            {
                SpotifyItem? oldItem = item;
                item = value;
                ImageSource = null;
                RequestMusicItemInfo();
                if (oldItem != value)
                {
                    NotifyPropertyChanged();
                }
            }
        }

        public MusicItemInfo Info
        {
            get => info;

            private set
            {
                SetProperty(ref info, value);
                SetImageSource();
            }
        }

        public string Name
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
            get => imageSource;
            private set => SetProperty(ref imageSource, value);
        }

        public void RequestMusicItemInfo()
        {
            if (Item is SpotifyItem si)
            {
                if (!taskMachine.TryIngest(_ => SpotifyItemPicker.MusicItemDictionary.GetValueAsync(si, info => Info = info)))
                {
                    throw new InvalidOperationException();
                }
            }
            else
            {
                Info = null;
            }
        }

        internal static void Refresh() => OnConnectionMade?.Invoke();

        private void SetImageSource()
        {
            string imageUrl = Info?.Images.OrderBy(i => i.Width * i.Height).FirstOrDefault()?.Url;
            ImageSource = Images.BuildFromUri(imageUrl);
        }
    }
}
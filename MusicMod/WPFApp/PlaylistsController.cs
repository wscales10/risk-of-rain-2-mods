using Spotify;
using System.Collections.ObjectModel;

namespace WPFApp
{
    public class PlaylistsController : NotifyPropertyChangedBase
    {
        private bool isEnabled;

        public bool IsEnabled
        {
            get => isEnabled;

            set
            {
                if (isEnabled && !value)
                {
                    Playlists.Clear();
                }

                SetProperty(ref isEnabled, value);
            }
        }

        public ObservableCollection<Playlist> Playlists { get; } = new();
    }
}
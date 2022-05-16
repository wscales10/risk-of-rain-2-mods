using Spotify;
using System;
using System.Linq;
using System.Windows;
using WPFApp.Controls.Wrappers.SaveResults;

namespace WPFApp.Controls.Wrappers
{
    internal class PlaylistWrapper : ControlWrapper<PlaylistRef, MyComboBox>
    {
        public PlaylistWrapper()
        {
            PlaylistNameUpdated += PlaylistWrapper_PlaylistNameUpdated;
            UIElement.ItemsSource = Info.Playlists;
            UIElement.DisplayMemberPath = nameof(PlaylistRef.Name);
        }

        private static event Action<Playlist> PlaylistNameUpdated;

        public override MyComboBox UIElement { get; } = new MyComboBox { VerticalAlignment = VerticalAlignment.Center, IsEditable = true, LinkText = false };

        public override string ValueString => UIElement.Text;

        public static void UpdatePlaylistName(Playlist playlist) => PlaylistNameUpdated?.Invoke(playlist);

        protected override void setValue(PlaylistRef value) => UIElement.Text = value.Name;

        protected override SaveResult<PlaylistRef> tryGetValue(bool trySave)
        {
            var playlist = (Playlist)UIElement.SelectedItem;

            if (playlist is null)
            {
                UIElement.SelectedItem = playlist = UIElement.Items.Cast<Playlist>().FirstOrDefault(p => p.Name == UIElement.Text);
            }

            return playlist is null ? (new(false)) : (new(new DynamicPlaylistRef(playlist)));
        }

        private void PlaylistWrapper_PlaylistNameUpdated(Playlist playlist)
        {
            if (playlist is not null && UIElement.SelectedItem is not null && ReferenceEquals(UIElement.SelectedItem, playlist))
            {
                UIElement.Text = playlist.Name;
            }
        }
    }
}
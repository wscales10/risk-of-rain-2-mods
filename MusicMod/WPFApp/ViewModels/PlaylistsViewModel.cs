using Spotify;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WPFApp.Controls.GridManagers;
using WPFApp.Controls.Rows;

namespace WPFApp.ViewModels
{
    internal class PlaylistsViewModel : RowViewModelBase<ObservableCollection<Playlist>>
    {
        public PlaylistsViewModel(ObservableCollection<Playlist> playlists, NavigationContext navigationContext) : base(playlists, navigationContext)
        {
            ExtraCommands = new[]
            {
                new ButtonContext { Label = "Add Playlist", Command = new ButtonCommand(_ => AddPlaylist()) }
            };

            TypedRowManager.BindTo(Item, AddPlaylist, r => r.Output);
        }

        public override string Title => "Custom Playlists:";

        public override IEnumerable<ButtonContext> ExtraCommands { get; }

        protected override RowManager<PlaylistRow> TypedRowManager { get; } = new();

        private PlaylistRow AddPlaylist(Playlist playlist = null) => TypedRowManager.Add(new PlaylistRow(NavigationContext) { Output = playlist ?? new Playlist() });
    }
}
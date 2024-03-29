﻿using Spotify;
using System;
using System.Collections.Generic;
using System.Linq;
using WPFApp.Controls.GridManagers;
using WPFApp.Controls.Rows;
using WPFApp.Controls.Wrappers;
using WPFApp.Controls.Wrappers.SaveResults;

namespace WPFApp.ViewModels
{
    internal class PlaylistViewModel : NamedViewModelBase<Playlist>
    {
        private readonly PlaylistsController playlistsController;

        public PlaylistViewModel(Playlist playlist, NavigationContext navigationContext, PlaylistsController playlistsController) : base(playlist, navigationContext)
        {
            this.playlistsController = playlistsController ?? throw new ArgumentNullException(nameof(playlistsController));
            Name = Item.Name;

            TypedRowManager.BindLooselyTo(Item, AddTrack, valuegetter);

            ExtraCommands = new[]
            {
                new ButtonContext { Label = "Add Track", Command = new ButtonCommand(_ => AddTrack()) }
            };

            SetPropertyDependency(nameof(AsString), TypedRowManager, nameof(TypedRowManager.Items));
        }

        public override string NameWatermark => "Untitled playlist";

        public override string Title => "Tracks:";

        public override IEnumerable<ButtonContext> ExtraCommands { get; }

        protected override RowManager<SpotifyItemRow> TypedRowManager { get; } = new();

        protected override bool ValidateName(string name) => name is not null && base.ValidateName(name) && !playlistsController.Playlists.Any(p => !ReferenceEquals(p, Item) && p.Name == name);

        protected override SaveResult<Playlist> ShouldAllowExit()
        {
            NameResult.MaybeOutput(name =>
            {
                if (NameResult.IsSuccess)
                {
                    Item.Name = name;
                    NotifyPropertyChanged(nameof(Name));
                    PlaylistWrapper.UpdatePlaylistName(Item);
                }
            });

            return base.ShouldAllowExit() & NameResult;
        }

        private static SaveResult<SpotifyItem> valuegetter(SpotifyItemRow row)
        {
            return (SaveResult<SpotifyItem>)row.TrySaveChanges();
        }

        private SpotifyItemRow AddTrack(SpotifyItem track = null)
        {
            return TypedRowManager.Add(new SpotifyItemRow() { Output = track });
        }
    }
}
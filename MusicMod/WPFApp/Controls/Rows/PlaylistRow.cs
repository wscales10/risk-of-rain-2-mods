using Spotify;
using WPFApp.ViewModels;

namespace WPFApp.Controls.Rows
{
    internal class PlaylistRow : ButtonRow<Playlist, PlaylistRow>
    {
        public PlaylistRow(NavigationContext navigationContext) : base(navigationContext, true)
        {
            SetPropertyDependency(nameof(ButtonContent), OutputViewModel, nameof(PlaylistViewModel.Name));
        }

        public override string ButtonContent => Output?.Name ?? "Untitled playlist";
    }
}
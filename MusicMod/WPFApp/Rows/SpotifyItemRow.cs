using Spotify;
using WPFApp.ViewModels;

namespace WPFApp.Rows
{
    internal class SpotifyItemRow : Row<SpotifyItem, SpotifyItemRow>
    {
        public SpotifyItemRow() : base(true)
        {
            DefinePropertyValidation(nameof(Output), new MyValidationRule<SpotifyItem>(output => output?.Type == SpotifyItemType.Track, _ => $"Must be a track"));
        }
    }
}
using Patterns;

namespace Spotify.Commands.Interfaces
{
    public interface ITransferCommand
    {
        IPattern<string> FromTrackId { get; set; }

        SpotifyItem Item { get; set; }

        Switch<int, string> Mapping { get; set; }

        int Map(int milliseconds);
    }
}
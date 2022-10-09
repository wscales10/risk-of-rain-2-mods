namespace Spotify.Commands
{
    public interface IOffset
    {
    }

    public class IndexOffset : IOffset
    {
        public int Position { get; set; }
    }

    public class ItemOffset : IOffset
    {
        public SpotifyItem Item { get; set; }
    }
}
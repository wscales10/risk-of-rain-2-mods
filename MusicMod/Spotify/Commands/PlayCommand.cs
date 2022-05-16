using System;
using System.Xml.Linq;
using Utils;

namespace Spotify.Commands
{
    public class PlayCommand : PlayCommandBase<PlayCommand>
    {
        public PlayCommand(SpotifyItemType type, string id) : base(type, id)
        {
        }

        public PlayCommand(ISpotifyItem item) : base(item)
        {
        }

        public PlayCommand()
        {
        }

        internal PlayCommand(XElement element) : base(element)
        {
        }

        private PlayCommand(ISpotifyItem item, TimeSpan? at) : base(item, at)
        {
        }

        public override PlayCommand AtMilliseconds(int ms) => new PlayCommand(Item, TimeSpan.FromMilliseconds(ms));

        public override PlayCommand AtSeconds(int s) => new PlayCommand(Item, TimeSpan.FromSeconds(s));
    }
}
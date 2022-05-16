using System;
using System.Xml.Linq;

namespace Spotify.Commands
{
    public class PlayOnceCommand : PlayCommandBase<PlayOnceCommand>
    {
        public PlayOnceCommand(SpotifyItemType type, string id) : base(type, id)
        {
        }

        public PlayOnceCommand(ISpotifyItem item) : base(item)
        {
        }

        public PlayOnceCommand()
        {
        }

        internal PlayOnceCommand(XElement element) : base(element)
        {
        }

        private PlayOnceCommand(ISpotifyItem item, TimeSpan? at) : base(item, at)
        {
        }

        public override PlayOnceCommand AtMilliseconds(int ms) => new PlayOnceCommand(Item, TimeSpan.FromMilliseconds(ms));

        public override PlayOnceCommand AtSeconds(int s) => new PlayOnceCommand(Item, TimeSpan.FromSeconds(s));
    }
}
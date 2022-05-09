using System;
using System.Xml.Linq;
using Utils;

namespace Spotify.Commands
{
    public class PlayOnceCommand : Command
    {
        public PlayOnceCommand(SpotifyItemType type, string id)
            : this(new SpotifyItem(type, id))
        {
        }

        public PlayOnceCommand(ISpotifyItem item) => Item = item;

        public PlayOnceCommand()
        {
        }

        internal PlayOnceCommand(XElement element)
        {
            Item = SpotifyItem.FromXml(element.Element(nameof(Item)).OnlyChild());
            At = TimeSpan.FromMilliseconds(int.Parse(element.Attribute(nameof(Milliseconds)).Value));
        }

        private PlayOnceCommand(ISpotifyItem item, TimeSpan? at) : this(item) => At = at;

        public ISpotifyItem Item { get; set; }

        public TimeSpan? At { get; set; }

        public int Milliseconds => (int)(At ?? TimeSpan.Zero).TotalMilliseconds;

        public PlayOnceCommand AtSeconds(int s) => new PlayOnceCommand(Item, TimeSpan.FromSeconds(s));

        public PlayOnceCommand AtMilliseconds(int ms) => new PlayOnceCommand(Item, TimeSpan.FromMilliseconds(ms));

        public override string ToString()
        {
            var timePart = Milliseconds == 0 ? null : $" at {(At ?? TimeSpan.Zero).ToCompactString()}";
            return base.ToString() + $"({Item}{timePart})";
        }

        protected override void AddDetail(XElement element)
        {
            element.SetAttributeValue(nameof(Milliseconds), Milliseconds);
            element.Add(new XElement(nameof(Item), Item.ToXml()));
        }
    }
}
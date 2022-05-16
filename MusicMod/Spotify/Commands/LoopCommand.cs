using System;
using System.Xml.Linq;
using Utils;

namespace Spotify.Commands
{
    public abstract class PlayCommandBase<TCommand> : Command
        where TCommand : PlayCommandBase<TCommand>
    {
        protected PlayCommandBase(SpotifyItemType type, string id)
            : this(new SpotifyItem(type, id))
        {
        }

        protected PlayCommandBase()
        {
        }

        protected PlayCommandBase(XElement element)
        {
            Item = SpotifyItem.FromXml(element.Element(nameof(Item)).OnlyChild());

            var at = element.Attribute(nameof(OptionalMilliseconds));

            if (!(at is null))
            {
                At = TimeSpan.FromMilliseconds(int.Parse(at.Value));
            }
        }

        protected PlayCommandBase(ISpotifyItem item, TimeSpan? at = null)
        {
            Item = item;
            At = at;
        }

        public ISpotifyItem Item { get; set; }

        public TimeSpan? At { get; set; }

        public int Milliseconds => OptionalMilliseconds ?? 0;

        protected int? OptionalMilliseconds => (int?)(At?.TotalMilliseconds);

        public abstract TCommand AtMilliseconds(int ms);

        public abstract TCommand AtSeconds(int s);

        public override string ToString()
        {
            var timePart = Milliseconds == 0 ? null : $" at {(At ?? TimeSpan.Zero).ToCompactString()}";
            return base.ToString() + $"({Item}{timePart})";
        }

        protected override void AddDetail(XElement element)
        {
            if (!(At is null))
            {
                element.SetAttributeValue(nameof(Milliseconds), OptionalMilliseconds);
            }

            element.Add(new XElement(nameof(Item), Item.ToXml()));
        }
    }

    public class LoopCommand : PlayCommandBase<LoopCommand>
    {
        public LoopCommand(SpotifyItemType type, string id) : base(type, id)
        {
        }

        public LoopCommand(ISpotifyItem item) : base(item)
        {
        }

        public LoopCommand()
        {
        }

        internal LoopCommand(XElement element) : base(element)
        {
        }

        private LoopCommand(ISpotifyItem item, TimeSpan? at) : base(item, at)
        {
        }

        public override LoopCommand AtMilliseconds(int ms) => new LoopCommand(Item, TimeSpan.FromMilliseconds(ms));

        public override LoopCommand AtSeconds(int s) => new LoopCommand(Item, TimeSpan.FromSeconds(s));
    }
}
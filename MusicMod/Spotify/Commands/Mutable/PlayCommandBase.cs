using Spotify.Commands.Interfaces;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Utils;

namespace Spotify.Commands.Mutable
{
    public abstract class PlayCommandBase<TCommand> : Command, IPlayCommand where TCommand : PlayCommandBase<TCommand>
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

        IPlayCommand IPlayCommand.AtMilliseconds(int ms) => AtMilliseconds(ms);

        IPlayCommand IPlayCommand.AtSeconds(int s) => AtSeconds(s);

        protected override void AddDetail(XElement element)
        {
            if (!(At is null))
            {
                element.SetAttributeValue(nameof(Milliseconds), OptionalMilliseconds);
            }

            element.Add(new XElement(nameof(Item), Item.ToXml()));
        }
    }
}
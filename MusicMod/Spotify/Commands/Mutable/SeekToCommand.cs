using Spotify.Commands.Interfaces;
using System;
using System.Xml.Linq;
using Utils;

namespace Spotify.Commands.Mutable
{
    public class SeekToCommand : Command, ISeekToCommand
    {
        public SeekToCommand() : this(TimeSpan.Zero)
        {
        }

        internal SeekToCommand(XElement element)
        {
            At = TimeSpan.FromMilliseconds(int.Parse(element.Attribute(nameof(Milliseconds)).Value));
        }

        private SeekToCommand(TimeSpan at) => At = at;

        public int Milliseconds => (int)At.TotalMilliseconds;

        public TimeSpan At { get; set; }

        public static SeekToCommand AtSeconds(int s) => new SeekToCommand(TimeSpan.FromSeconds(s));

        public static SeekToCommand AtMilliseconds(int ms) => new SeekToCommand(TimeSpan.FromMilliseconds(ms));

        public override string ToString()
        {
            return base.ToString() + $"({At.ToCompactString()})";
        }

        public override IReadOnlyCommand ToReadOnly()
        {
            return new ReadOnlySeekToCommand(this);
        }

        protected override void AddDetail(XElement element)
        {
            element.SetAttributeValue("Milliseconds", Milliseconds);
        }
    }
}
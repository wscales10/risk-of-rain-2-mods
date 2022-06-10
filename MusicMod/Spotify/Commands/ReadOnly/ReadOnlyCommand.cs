using Spotify.Commands.Interfaces;
using Spotify.Commands.Mutable;
using System.Xml.Linq;

namespace Spotify.Commands.ReadOnly
{
    public abstract class ReadOnlyCommand : IReadOnlyCommand
    {
        private readonly string xml;

        protected ReadOnlyCommand(Command mutable) => xml = mutable.ToXml().ToString();

        public IReadOnlyCommand ToReadOnly() => this;

        public XElement ToXml() => XElement.Parse(xml);
    }
}
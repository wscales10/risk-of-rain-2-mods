using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using Utils;

namespace Spotify
{
    public class PlaylistRef : IXmlExportable, ISpotifyItem
    {
        public PlaylistRef(string name) => Name = name;

        public PlaylistRef(XElement xml) : this(xml.Attribute("name").Value)
        {
        }

        protected PlaylistRef()
        {
        }

        public virtual string Name { get; }

        public XElement ToXml() => ToXml(Name);

        internal static XElement ToXml(string name)
        {
            var output = new XElement(nameof(Playlist));
            output.SetAttributeValue("name", name);
            return output;
        }
    }

    public class DynamicPlaylistRef : PlaylistRef
    {
        private readonly Playlist playlist;

        public DynamicPlaylistRef(Playlist playlist) => this.playlist = playlist;

        public override string Name => playlist.Name;
    }

    public class Playlist : Collection<SpotifyItem>, IXmlExportable
    {
        public Playlist()
        {
        }

        public Playlist(string name) => Name = name;

        public Playlist(XElement xml) : base(xml.Elements().Select(SpotifyItem.FromXml).Cast<SpotifyItem>().ToList())
        {
            Name = xml.Attribute("name")?.Value;
        }

        public string Name { get; set; }

        public Playlist DeepClone() => new Playlist(ToXml());

        public XElement ToXml()
        {
            var output = PlaylistRef.ToXml(Name);

            foreach (var item in this)
            {
                if (item is null)
                {
                    output.Add(new XElement("null"));
                }
                else
                {
                    output.Add(item.ToXml());
                }
            }

            return output;
        }
    }
}
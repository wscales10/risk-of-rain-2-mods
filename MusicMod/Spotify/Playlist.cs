using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using Utils;

namespace Spotify
{
    public class Playlist : Collection<SpotifyItem>, IXmlExportable, ISpotifyItem
    {
        public Playlist(XElement xml) : base(xml.Elements().Select(e => new SpotifyItem(e)).ToList())
        {
            Name = xml.Attribute("name")?.Value;
        }

        public string Name { get; set; }

        public XElement ToXml()
        {
            var output = new XElement("Playlist");
            output.SetAttributeValue("name", Name);

            foreach (var item in this)
            {
                output.Add(item.ToXml());
            }

            return output;
        }
    }
}
using System;
using System.Xml;
using System.Xml.Linq;
using Utils;

namespace Spotify
{
    public enum SpotifyItemType
    {
        Track,

        Playlist,

        Album,

        Artist,

        User
    }

    public class SpotifyItem : IEquatable<SpotifyItem>, ISpotifyItem
    {
        public SpotifyItem(SpotifyItemType type, string id)
        {
            Type = type;
            Id = id ?? throw new ArgumentNullException(nameof(id));
        }

        internal SpotifyItem(XElement element) : this(element.Attribute(nameof(Type)).Value.AsEnum<SpotifyItemType>(), element.Attribute(nameof(Id)).Value)
        {
        }

        public string Id { get; }

        public SpotifyItemType Type { get; }

        public static bool operator ==(SpotifyItem item1, SpotifyItem item2) => item1 is null ? item2 is null : item1.Equals(item2);

        public static bool operator !=(SpotifyItem item1, SpotifyItem item2) => !(item1 == item2);

        public override bool Equals(object o) => Equals(o as SpotifyItem);

        public bool Equals(SpotifyItem item)
        {
            if (item is null)
            {
                return false;
            }

            if (ReferenceEquals(this, item))
            {
                return true;
            }

            if (GetType() != item.GetType())
            {
                return false;
            }

            return Type == item.Type && Id == item.Id;
        }

        public XElement ToXml()
        {
            var element = new XElement(nameof(SpotifyItem));

            foreach (var property in GetType().GetProperties())
            {
                element.SetAttributeValue(property.Name, property.GetValue(this));
            }

            return element;
        }

        public override int GetHashCode() => Id.GetHashCode();

        public Uri GetUri() => new Uri($"spotify:{ToUriString()}");

        public Uri GetUrl() => new Uri($"https://open.spotify.com/{Type.ToString().ToLower()}/{Id}");

        public override string ToString() => ToUriString();

        internal static ISpotifyItem FromXml(XElement element)
        {
            switch (element.Name.ToString())
            {
                case nameof(SpotifyItem):
                    return new SpotifyItem(element);

                case nameof(Playlist):
                    return new PlaylistRef(element);

                case "null":
                    return null;

                default:
                    throw new XmlException();
            }
        }

        private string ToUriString() => $"{Type.ToString().ToLower()}:{Id}";
    }
}
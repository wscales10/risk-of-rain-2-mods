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

    public struct SpotifyItem : IEquatable<SpotifyItem>, ISpotifyItem
    {
        public SpotifyItem(SpotifyItemType type, string id) : this()
        {
            Type = type;
            Id = id ?? throw new ArgumentNullException(nameof(id));
        }

        internal SpotifyItem(XElement element) : this(element.Attribute(nameof(Type)).Value.AsEnum<SpotifyItemType>(), element.Attribute(nameof(Id)).Value)
        {
        }

        public string Id { get; }

        public SpotifyItemType Type { get; }

        public static bool operator !=(SpotifyItem mi1, SpotifyItem mi2) => !mi1.Equals(mi2);

        public static bool operator ==(SpotifyItem mi1, SpotifyItem mi2) => mi1.Equals(mi2);

        public override bool Equals(object o) => o is SpotifyItem mi && Equals(mi);

        public bool Equals(SpotifyItem mi) => Type == mi.Type && Id == mi.Id;

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
                    return new Playlist(element);

                default:
                    throw new XmlException();
            }
        }

        private string ToUriString() => $"{Type.ToString().ToLower()}:{Id}";
    }
}
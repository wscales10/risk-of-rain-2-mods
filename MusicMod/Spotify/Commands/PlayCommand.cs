using System;
using System.Xml.Linq;
using Utils;

namespace Spotify.Commands
{
	public class PlayCommand : Command
	{
		internal PlayCommand(XElement element)
		{
			Item = new SpotifyItem(element.Element(nameof(Item)));
			At = TimeSpan.FromMilliseconds(int.Parse(element.Attribute(nameof(Milliseconds)).Value));
		}

		private PlayCommand(SpotifyItem? item, TimeSpan? at)
		{
			Item = item;
			At = at;
		}

		public PlayCommand(SpotifyItemType type, string id)
			: this(new SpotifyItem(type, id))
		{
		}

		public PlayCommand(SpotifyItem? item = null) : this(item, null)
		{
		}

		public PlayCommand AtSeconds(int s) => new PlayCommand(Item, TimeSpan.FromSeconds(s));

		public PlayCommand AtMilliseconds(int ms) => new PlayCommand(Item, TimeSpan.FromMilliseconds(ms));

		public SpotifyItem? Item { get; set; }

		public TimeSpan? At { get; set; }

		public int Milliseconds => (int)(At ?? TimeSpan.Zero).TotalMilliseconds;

		protected override void AddDetail(XElement element)
		{
			element.SetAttributeValue(nameof(Milliseconds), Milliseconds);
			element.Add(Item.Value.FillAttributesTo(new XElement(nameof(Item))));
		}

		public override string ToString()
		{
			var timePart = Milliseconds == 0 ? null : $" at {(At ?? TimeSpan.Zero).ToCompactString()}";
			return base.ToString() + $"({Item}{timePart})";
		}
	}
}


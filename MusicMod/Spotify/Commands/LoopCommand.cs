using System;
using System.Xml.Linq;
using Utils;

namespace Spotify.Commands
{
	public class LoopCommand : Command
	{
		public LoopCommand(SpotifyItemType type, string id)
			: this(new SpotifyItem(type, id))
		{
		}

		public LoopCommand(SpotifyItem? item) => Item = item;

		public LoopCommand()
		{
		}

		internal LoopCommand(XElement element)
		{
			Item = new SpotifyItem(element.Element(nameof(Item)));
			At = TimeSpan.FromMilliseconds(int.Parse(element.Attribute(nameof(Milliseconds)).Value));
		}

		private LoopCommand(SpotifyItem? item, TimeSpan? at) : this(item) => At = at;

		public SpotifyItem? Item { get; set; }

		public TimeSpan? At { get; set; }

		public int Milliseconds => (int)(At ?? TimeSpan.Zero).TotalMilliseconds;

		public LoopCommand AtSeconds(int s) => new LoopCommand(Item, TimeSpan.FromSeconds(s));

		public LoopCommand AtMilliseconds(int ms) => new LoopCommand(Item, TimeSpan.FromMilliseconds(ms));

		public override string ToString()
		{
			var timePart = Milliseconds == 0 ? null : $" at {(At ?? TimeSpan.Zero).ToCompactString()}";
			return base.ToString() + $"({Item}{timePart})";
		}

		protected override void AddDetail(XElement element)
		{
			element.SetAttributeValue(nameof(Milliseconds), Milliseconds);
			element.Add(Item.Value.FillAttributesTo(new XElement(nameof(Item))));
		}
	}
}
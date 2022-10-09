using System;
using System.Xml.Linq;
using Utils;

namespace Spotify.Commands
{
	public abstract class PlayCommandBase : Command
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
			var itemElement = element.Element(nameof(Item))?.OnlyChild();

			if (!(itemElement is null))
			{
				Item = SpotifyItem.FromXml(itemElement);
			}

			var at = element.Attribute(nameof(Milliseconds));

			if (!(at is null))
			{
				At = TimeSpan.FromMilliseconds(int.Parse(at.Value));
			}

			var offset = element.Element(nameof(Offset));

			if (!(offset is null))
			{
				if (int.TryParse(offset.Value, out int positionOffset))
				{
					Offset = new IndexOffset { Position = positionOffset };
				}
				else
				{
					Offset = new ItemOffset { Item = (SpotifyItem)SpotifyItem.FromXml(offset.OnlyChild()) };
				}
			}
		}

		protected PlayCommandBase(ISpotifyItem item, TimeSpan? at = null)
		{
			Item = item;
			At = at;
		}

		public ISpotifyItem Item { get; set; }

		public TimeSpan? At { get; set; }

		public IOffset Offset { get; set; }

		public int Milliseconds => OptionalMilliseconds ?? 0;

		protected int? OptionalMilliseconds => (int?)(At?.TotalMilliseconds);

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

			if (!(Item is null))
			{
				element.Add(new XElement(nameof(Item), Item.ToXml()));
			}

			if (!(Offset is null))
			{
				object offset;

				switch (Offset)
				{
					case IndexOffset indexOffset:
						offset = indexOffset.Position;
						break;

					case ItemOffset itemOffset:
						offset = itemOffset.Item.ToXml();
						break;

					default:
						offset = null;
						break;
				}

				element.Add(new XElement(nameof(Offset), offset));
			}
		}
	}

	public abstract class PlayCommandBase<TCommand> : PlayCommandBase
		where TCommand : PlayCommandBase<TCommand>
	{
		protected PlayCommandBase(SpotifyItemType type, string id) : base(type, id)
		{
		}

		protected PlayCommandBase()
		{
		}

		protected PlayCommandBase(XElement element) : base(element)
		{
		}

		protected PlayCommandBase(ISpotifyItem item, TimeSpan? at = null) : base(item, at)
		{
		}

		public abstract TCommand AtMilliseconds(int ms);

		public abstract TCommand AtSeconds(int s);
	}
}
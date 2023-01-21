using Maths;
using Patterns;
using Patterns.Patterns;
using System.Xml.Linq;
using Utils;

namespace Spotify.Commands
{
	public class TransferCommand : Command
	{
		public TransferCommand(SpotifyItemType type, string id, Switch<int, string> mapping, IPattern<string> fromTrackId = null)
			: this(new SpotifyItem(type, id), mapping, fromTrackId)
		{
		}

		public TransferCommand(SpotifyItem item, Switch<int, string> mapping = null, IPattern<string> fromTrackId = null)
		{
			Item = item;
			Mapping = mapping ?? new Switch<int, string>();
			FromTrackId = fromTrackId ?? ConstantPattern<string>.True;
		}

		public TransferCommand(SpotifyItemType type, string id, string mapping, IPattern<string> fromTrackId = null)
			: this(new SpotifyItem(type, id), mapping, fromTrackId)
		{
		}

		public TransferCommand(SpotifyItem item, string mapping, IPattern<string> fromTrackId = null) : this(item, new Switch<int, string>(mapping), fromTrackId)
		{
		}

		public TransferCommand() : this((SpotifyItem)null)
		{
		}

		internal TransferCommand(XElement element)
		{
			Item = (SpotifyItem)SpotifyItem.FromXml(element.Element(nameof(Item)).OnlyChild(true));
			Mapping = Switch<int, string>.Parse(element.Element(nameof(Mapping)).Element("Switch"), e => e.Value, PatternParser.Instance);
			FromTrackId = PatternParser.Instance.Parse<string>(element.Element(nameof(FromTrackId)).OnlyChild(true));
		}

		public IPattern<string> FromTrackId { get; set; }

		public Switch<int, string> Mapping { get; set; }

		public SpotifyItem Item { get; set; }

		public static int Map(int milliseconds, string symbolicExpression)
		{
			return (int)Methods.Evaluate(symbolicExpression, "ms", milliseconds);
		}

		public int Map(int milliseconds)
		{
			return Map(milliseconds, Mapping.GetOut(milliseconds));
		}

		protected override void AddDetail(XElement element)
		{
			element.Add(new XElement(nameof(Item), Item?.ToXml()));
			element.Add(new XElement(nameof(FromTrackId), FromTrackId?.ToXml()));
			element.Add(new XElement(nameof(Mapping), Mapping?.ToXml(se => new XElement("Expr", se))));
		}
	}
}
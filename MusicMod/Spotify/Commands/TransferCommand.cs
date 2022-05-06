using MathNet.Symbolics;
using MyRoR2;
using Patterns;
using Patterns.Patterns;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using Utils;

namespace Spotify.Commands
{
	public delegate SymbolicExpression MathFunc(SymbolicExpression ms);

	public class TransferCommand : Command
	{
		public TransferCommand(SpotifyItemType type, string id, Switch<int, SymbolicExpression> mapping, IPattern<string> fromTrackId = null)
			: this(new SpotifyItem(type, id), mapping, fromTrackId)
		{
		}

		public TransferCommand(SpotifyItem? item, Switch<int, SymbolicExpression> mapping = null, IPattern<string> fromTrackId = null)
		{
			Item = item;
			Mapping = mapping ?? new Switch<int, SymbolicExpression>();
			FromTrackId = fromTrackId ?? ConstantPattern<string>.True;
		}

		public TransferCommand(SpotifyItemType type, string id, SymbolicExpression mapping, IPattern<string> fromTrackId = null)
			: this(new SpotifyItem(type, id), mapping, fromTrackId)
		{
		}

		public TransferCommand(SpotifyItem? item, SymbolicExpression mapping, IPattern<string> fromTrackId = null) : this(item, new Switch<int, SymbolicExpression>(mapping), fromTrackId)
		{
		}

		public TransferCommand() : this((SpotifyItem?)null)
		{
		}

		internal TransferCommand(XElement element)
		{
			Item = new SpotifyItem(element.Element(nameof(Item)));
			Mapping = Switch<int, SymbolicExpression>.Parse(element.Element(nameof(Mapping)).Element("Switch"), e => SymbolicExpression.ParseMathML(e.Value), RoR2PatternParser.Instance);
			FromTrackId = PatternParser.Instance.Parse<string>(element.Element(nameof(FromTrackId)).OnlyChild());
		}

		[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
		public static SymbolicExpression ms { get; } = SymbolicExpression.Variable("ms");

		public IPattern<string> FromTrackId { get; set; }

		public Switch<int, SymbolicExpression> Mapping { get; set; }

		public SpotifyItem? Item { get; set; }

		public static int Map(int milliseconds, SymbolicExpression symbolicExpression)
		{
			return (int)symbolicExpression.Evaluate(new Dictionary<string, FloatingPoint> { { ms.ToString(), milliseconds } }).RealValue;
		}

		public int Map(int milliseconds)
		{
			return Map(milliseconds, Mapping.GetOut(milliseconds));
		}

		protected override void AddDetail(XElement element)
		{
			element.Add(Item.Value.FillAttributesTo(new XElement(nameof(Item))));
			element.Add(new XElement(nameof(FromTrackId), FromTrackId.ToXml()));
			element.Add(new XElement(nameof(Mapping), Mapping.ToXml(se => new XElement("Expr", se.ToMathML()))));
		}
	}
}
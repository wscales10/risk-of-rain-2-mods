using Maths;
using MyRoR2;
using Patterns;
using Patterns.Patterns;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using Utils;

namespace Spotify.Commands
{
    //public delegate SymbolicExpression MathFunc(SymbolicExpression ms);

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
            Item = new SpotifyItem(element.Element(nameof(Item)).OnlyChild());
            Mapping = Switch<int, string>.Parse(element.Element(nameof(Mapping)).Element("Switch"), e => e.Value, RoR2PatternParser.Instance);
            FromTrackId = PatternParser.Instance.Parse<string>(element.Element(nameof(FromTrackId)).OnlyChild());
        }

        //[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
        //public static MathExpression ms { get; } = MathExpression.Variable("ms");

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
            element.Add(new XElement(nameof(Item), Item.ToXml()));
            element.Add(new XElement(nameof(FromTrackId), FromTrackId.ToXml()));
            element.Add(new XElement(nameof(Mapping), Mapping.ToXml(se => new XElement("Expr", se))));
        }
    }
}
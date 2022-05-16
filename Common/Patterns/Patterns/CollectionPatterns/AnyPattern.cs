using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Patterns.Patterns.CollectionPatterns
{
    public class AnyPattern<T> : CollectionPattern<T>, IOnlyChildPattern<T>
    {
        public AnyPattern(IPattern<T> predicate)
        {
            Child = predicate;
        }

        public AnyPattern()
        {
        }

        public IPattern<T> Child { get; set; }

        public override bool IsMatch(IList<T> value) => value?.Any(Child.IsMatch) ?? false;

        public override XElement ToXml() => new XElement("Any", Child.ToXml());
    }
}
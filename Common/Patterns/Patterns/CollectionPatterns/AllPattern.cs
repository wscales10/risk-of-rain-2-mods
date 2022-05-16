using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Patterns.Patterns.CollectionPatterns
{
    public class AllPattern<T> : CollectionPattern<T>, IOnlyChildPattern<T>
    {
        public AllPattern(IPattern<T> predicate)
        {
            Child = predicate;
        }

        public AllPattern()
        {
        }

        public IPattern<T> Child { get; set; }

        public override bool IsMatch(IList<T> value) => value?.All(Child.IsMatch) ?? true;

        public override XElement ToXml() => new XElement("All", Child.ToXml());
    }
}
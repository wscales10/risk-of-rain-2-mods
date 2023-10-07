using System;
using System.Collections.ObjectModel;

namespace Utils
{
    public class ConcreteKeyedCollection<TKey, TItem> : KeyedCollection<TKey, TItem>
    {
        private readonly Func<TItem, TKey> getKeyForItem;

        public ConcreteKeyedCollection(Func<TItem, TKey> getKeyForItem)
        {
            this.getKeyForItem = getKeyForItem ?? throw new ArgumentNullException(nameof(getKeyForItem));
        }

        public bool TryGetValue(TKey key, out TItem value)
        {
            if (Contains(key))
            {
                value = this[key];
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        protected override TKey GetKeyForItem(TItem item)
        {
            return getKeyForItem(item);
        }
    }
}
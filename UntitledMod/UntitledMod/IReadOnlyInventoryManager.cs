using RoR2;
using System.Collections.Specialized;

namespace UntitledMod
{
    public interface IReadOnlyInventoryManager
    {
        event NotifyCollectionChangedEventHandler BannedItemsChanged;

        bool WantsToKeep(ItemIndex itemIndex);

        bool IsAllowed(ItemIndex itemIndex);
    }
}
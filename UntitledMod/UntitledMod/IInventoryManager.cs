using RoR2;

namespace UntitledMod
{
    public interface IInventoryManager : IReadOnlyInventoryManager
    {
        void OnPickupItem(ItemIndex itemIndex);

        void OnLoseItem(ItemIndex itemIndex);
    }
}
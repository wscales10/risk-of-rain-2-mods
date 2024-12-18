using RoR2;

namespace UntitledMod
{
    public interface IReadOnlyInventoryManager
    {
        bool WantsToKeep(ItemIndex itemIndex);

        bool IsAllowed(ItemIndex itemIndex);
    }
}
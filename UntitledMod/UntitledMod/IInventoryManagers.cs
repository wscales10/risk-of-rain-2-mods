using RoR2;
using System.Collections.Generic;

namespace UntitledMod
{
    public interface IInventoryManagers : IEnumerable<IInventoryManager>
    {
        void Add(CharacterMaster characterMaster);

        void Reset();

        bool TryGetValue(CharacterMaster characterMaster, out IInventoryManager inventoryManager);

        bool TryGetValue(Inventory inventory, out IInventoryManager inventoryManager);
    }
}
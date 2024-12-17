using RoR2;

namespace UntitledMod
{
    public class InventoriesInfo
    {
        private readonly Writer writer;

        public InventoriesInfo(Writer writer)
        {
            this.writer = writer;
        }

        public bool Lookup(CharacterMaster characterMaster, out IReadOnlyInventoryManager inventoryManager)
        {
            if(this.writer.TryGetInventoryManager(characterMaster, out var output))
            {
                inventoryManager = output;
                return true;
            }
            else
            {
                inventoryManager = null;
                return false;
            }
        }
    }
}
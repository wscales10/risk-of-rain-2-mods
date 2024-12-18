using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UntitledMod
{
    public class InventoryManagers : IInventoryManagers
    {
        private readonly IDictionary<CharacterMaster, IInventoryManager> dictionary = new Dictionary<CharacterMaster, IInventoryManager>();

        private readonly ICustomLogger logger;

        private readonly Func<IInventoryManager> inventoryManagerFactory;

        public InventoryManagers(ICustomLogger logger, Func<IInventoryManager> inventoryManagerFactory)
        {
            this.logger = logger;
            this.inventoryManagerFactory = inventoryManagerFactory;
        }

        public IEnumerator<IInventoryManager> GetEnumerator() => this.dictionary.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public bool TryGetValue(CharacterMaster characterMaster, out IInventoryManager inventoryManager)
        {
            this.logger.LogMethodCall();
            if (characterMaster is null)
            {
                inventoryManager = null;
                return false;
            }

            return this.dictionary.TryGetValue(characterMaster, out inventoryManager);
        }

        public bool TryGetValue(Inventory inventory, out IInventoryManager inventoryManager)
        {
            var characterMaster = this.dictionary.Keys.SingleOrDefault(m => m.inventory == inventory);
            return this.TryGetValue(characterMaster, out inventoryManager);
        }

        public void Add(CharacterMaster characterMaster) => this.dictionary.Add(characterMaster, this.inventoryManagerFactory());

        public void Reset() => this.dictionary.Clear();
    }
}
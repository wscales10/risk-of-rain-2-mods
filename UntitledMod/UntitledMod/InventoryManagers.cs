using R2API.Utils;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UntitledMod.Context;

namespace UntitledMod
{
    public class InventoryManagers : IInventoryManagers
    {
        private readonly ICustomLogger logger;

        private readonly IRoR2Context gameContext;

        private readonly VisibleDamageItemsProvider visibleDamageItemsProvider;

        public InventoryManagers(ICustomLogger logger, IRoR2Context gameContext, VisibleDamageItemsProvider visibleDamageItemsProvider)
        {
            this.logger = logger;
            this.gameContext = gameContext;
            this.visibleDamageItemsProvider = visibleDamageItemsProvider;
        }

        public IEnumerator<IInventoryManager> GetEnumerator() => PlayerCharacterMasterController.instances.Select(x => x.GetComponent<InventoryManager>()).Cast<IInventoryManager>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public bool TryGetValue(CharacterMaster characterMaster, out IInventoryManager inventoryManager)
        {
            this.logger.LogMethodCall();
            if (characterMaster is null)
            {
                inventoryManager = null;
                return false;
            }

            var playerCharacterMasterController = PlayerCharacterMasterController.instances.FirstOrDefault(x => x.master == characterMaster);

            if (playerCharacterMasterController is null)
            {
                inventoryManager = null;
                return false;
            }

            inventoryManager = playerCharacterMasterController.GetComponent<InventoryManager>();
            return inventoryManager != null;
        }

        public bool TryGetValue(Inventory inventory, out IInventoryManager inventoryManager)
        {
            this.logger.LogMethodCall();
            if (inventory is null)
            {
                inventoryManager = null;
                return false;
            }

            var playerCharacterMasterController = PlayerCharacterMasterController.instances.FirstOrDefault(x => x.master?.inventory == inventory);

            if (playerCharacterMasterController is null)
            {
                inventoryManager = null;
                return false;
            }

            inventoryManager = playerCharacterMasterController.GetComponent<InventoryManager>();
            return inventoryManager != null;
        }

        public IInventoryManager Add(PlayerCharacterMasterController player)
        {
            // TODO: can this run on the client, for a different player?
            return player.gameObject.AddComponent<InventoryManager>().Init(this.logger, this.gameContext, this.visibleDamageItemsProvider);
        }

        public void Reset() => PlayerCharacterMasterController.instances.ForEachTry(x => x.GetComponent<InventoryManager>()?.Reset());
    }
}
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UntitledMod
{
    public class UntitledMod
    {
        private readonly Dictionary<CharacterMaster, InventoryManager> inventoryManagers = new Dictionary<CharacterMaster, InventoryManager>();

        private readonly CustomLogger logger;

        private readonly Func<InventoryManager> inventoryManagerFactory;

        internal UntitledMod(CustomLogger logger, Func<InventoryManager> inventoryManagerFactory)
        {
            this.logger = logger;
            this.inventoryManagerFactory = inventoryManagerFactory;

            On.RoR2.ItemCatalog.Init += this.ItemCatalog_Init;
            On.RoR2.Run.Start += this.Run_Start;
            On.RoR2.Inventory.GiveItem_ItemIndex_int += this.Inventory_GiveItem_ItemIndex_int;
            On.RoR2.Inventory.RemoveItem_ItemIndex_int += this.Inventory_RemoveItem_ItemIndex_int;
            typeof(IL.RoR2.CostTypeCatalog).GetEvent("<Init>g__PayCostItems|5_1").AddHook(this, nameof(CostTypeCatalog_Init));
            IL.RoR2.LunarSunBehavior.FixedUpdate += this.LunarSunBehavior_FixedUpdate;
            On.RoR2.PlayerCharacterMasterController.Awake += this.PlayerCharacterMasterController_Awake;
        }

        public void CostTypeCatalog_Init(ILContext il)
        {
            this.logger.LogMethodCall();
            var c = new ILCursor(il);

            // Create an extra WeightedSelection
            c.GotoNext(
                x => x.MatchNewobj<WeightedSelection<ItemIndex>>(),
                x => x.Match(OpCodes.Stloc_S),
                x => x.Match(OpCodes.Ldsfld)
            );

            var newWeightedSelectionVariable = new VariableDefinition(c.Body.Variables[3].VariableType);
            c.Body.Variables.Add(newWeightedSelectionVariable);
            c.Index += 2;
            c.Emit(OpCodes.Ldc_I4_8);
            c.Emit(OpCodes.Newobj, typeof(WeightedSelection<ItemIndex>).GetConstructor(new[] { typeof(int) }));
            c.Emit(OpCodes.Stloc_S, newWeightedSelectionVariable);

            // Modify conditional expression
            c.GotoNext(x => x.MatchLdloc(5), x => x.MatchLdloc(9), x => x.MatchLdloc(10), x => x.MatchConvR4());
            c.Emit(OpCodes.Ldloc_S, newWeightedSelectionVariable);
            var label = il.DefineLabel(c.Previous);
            c.Emit(OpCodes.Br_S, (ILLabel)c.Previous.Previous.Operand);

            c.GotoPrev(x => x.MatchLdloc(3), x => x.Match(OpCodes.Br_S));
            c.Emit(OpCodes.Ldarg_1);
            c.Emit(OpCodes.Ldloc_S, il.GetVariable<ItemIndex>(9));
            c.EmitDelegate<Func<CostTypeDef.PayCostContext, ItemIndex, bool>>((context, itemIndex) => this.TryGetInventoryManager(context.activatorMaster, out var inventoryManager) && inventoryManager.WantsToKeep(itemIndex));
            c.Emit(OpCodes.Brtrue_S, label);

            // Take items from new weighted selection
            c.GotoNext(x => x.MatchLdloc(3), x => x.MatchLdloca(0), x => x.MatchLdloca(2), x => x.Match(OpCodes.Call));
            c.Index += 4;
            var takeItemsFromWeightedSelectionMethod = (MethodReference)c.Previous.Operand;
            c.Emit(OpCodes.Ldloc_S, newWeightedSelectionVariable);
            c.Emit(OpCodes.Ldloca_S, c.Body.Variables[0]);
            c.Emit(OpCodes.Ldloca_S, c.Body.Variables[2]);
            c.Emit(OpCodes.Call, takeItemsFromWeightedSelectionMethod);
        }

        private void LunarSunBehavior_FixedUpdate(ILContext il)
        {
            this.logger.LogMethodCall();
            var c = new ILCursor(il);

            // After shuffling the list, move deprioritised items after everything else
            c.GotoNext(
                x => x.MatchDup(),
                x => x.MatchLdarg(0),
                x => x.Match(OpCodes.Ldfld),
                x => x.MatchCall(typeof(Util), nameof(Util.ShuffleList))
            );

            c.Index += 4;

            c.Emit(OpCodes.Dup);
            c.Emit(OpCodes.Ldarg_0);

            c.EmitDelegate<Action<List<ItemIndex>, LunarSunBehavior>>((list, behavior) =>
            {
                if (!this.TryGetInventoryManager(behavior.body.master, out var inventoryManager))
                {
                    return;
                }

                var deprioritisedItems = list.Where(inventoryManager.WantsToKeep).ToArray();

                foreach (var item in deprioritisedItems)
                {
                    list.Remove(item);
                }

                list.AddRange(deprioritisedItems);
            });
        }

        private void ItemCatalog_Init(On.RoR2.ItemCatalog.orig_Init orig)
        {
            orig();
            InventoryManager.Init();
        }

        private void PlayerCharacterMasterController_Awake(On.RoR2.PlayerCharacterMasterController.orig_Awake orig, PlayerCharacterMasterController self)
        {
            this.logger.LogMethodCall();
            orig(self);
            this.inventoryManagers.Add(self.master, this.inventoryManagerFactory());
        }

        private void Inventory_GiveItem_ItemIndex_int(On.RoR2.Inventory.orig_GiveItem_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int count)
        {
            this.OnPickupItem(self, itemIndex);
            orig(self, itemIndex, count);
        }

        private void Inventory_RemoveItem_ItemIndex_int(On.RoR2.Inventory.orig_RemoveItem_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int count)
        {
            try
            {
                orig(self, itemIndex, count);
            }
            finally
            {
                if (self.itemStacks[(int)itemIndex] == 0)
                {
                    this.OnLoseItem(self, itemIndex);
                }
            }
        }

        private void OnPickupItem(Inventory inventory, ItemIndex itemIndex)
        {
            if (this.TryGetInventoryManager(inventory, out var inventoryManager))
            {
                inventoryManager.OnPickupItem(itemIndex);
            }
        }

        private void OnLoseItem(Inventory inventory, ItemIndex itemIndex)
        {
            if (this.TryGetInventoryManager(inventory, out var inventoryManager))
            {
                inventoryManager.OnLoseItem(itemIndex);
            }
        }

        private bool TryGetInventoryManager(CharacterMaster characterMaster, out InventoryManager inventoryManager)
        {
            this.logger.LogMethodCall();
            if (characterMaster is null)
            {
                inventoryManager = null;
                return false;
            }

            return this.inventoryManagers.TryGetValue(characterMaster, out inventoryManager);
        }

        private bool TryGetInventoryManager(Inventory inventory, out InventoryManager inventoryManager)
        {
            var characterMaster = this.inventoryManagers.Keys.SingleOrDefault(m => m.inventory == inventory);
            return this.TryGetInventoryManager(characterMaster, out inventoryManager);
        }

        private void Run_Start(On.RoR2.Run.orig_Start orig, Run self)
        {
            this.logger.LogMethodCall();
            this.inventoryManagers.Clear();
            orig(self);
        }
    }
}
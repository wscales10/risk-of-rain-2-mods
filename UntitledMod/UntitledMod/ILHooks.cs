using Mono.Cecil.Cil;
using Mono.Cecil;
using MonoMod.Cil;
using RoR2;
using System;
using System.Collections.Generic;

namespace UntitledMod
{
    public partial class Reader
    {
        public void CostTypeCatalog_PayCostItems(ILContext il)
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
            c.EmitDelegate<Func<CostTypeDef.PayCostContext, ItemIndex, bool>>((context, itemIndex) => this.inventoriesInfo.Lookup(context.activatorMaster, out var inventoryManager) && inventoryManager.WantsToKeep(itemIndex));
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

        private void CharacterMaster_TryCloverVoidUpgrades(ILContext il)
        {
            this.logger.LogMethodCall();
            var c = new ILCursor(il);

            c.GotoNext(
                x => x.MatchLdloc(2),
                x => x.MatchLdarg(0),
                x => x.Match(OpCodes.Ldfld),
                x => x.MatchCall(typeof(Util), nameof(Util.ShuffleList))
            );

            c.Index += 4;

            c.Emit(OpCodes.Ldloc_2);
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<List<ItemIndex>, CharacterMaster>>(this.DeprioritiseItemsInList);
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
            c.Emit(OpCodes.Ldfld, typeof(LunarSunBehavior).GetField(nameof(LunarSunBehavior.body)));
            c.Emit<CharacterBody>(OpCodes.Callvirt, $"get_{nameof(CharacterBody.master)}");
            c.EmitDelegate<Action<List<ItemIndex>, CharacterMaster>>(this.DeprioritiseItemsInList);
        }
    }
}
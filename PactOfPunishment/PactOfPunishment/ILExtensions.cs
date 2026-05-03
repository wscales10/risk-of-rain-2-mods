using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace PactOfPunishment
{
    public static class ILExtensions
    {
        public static void GotoLast(this ILCursor c, params Func<Instruction, bool>[] predicates)
        {
            c.Index = c.Instrs.Count - 1;
            c.GotoPrev(predicates);
        }

        public static void GotoLast(this ILCursor c, MoveType moveType, params Func<Instruction, bool>[] predicates)
        {
            c.Index = c.Instrs.Count - 1;
            c.GotoPrev(moveType, predicates);
        }

        public static void InterceptLoadField<TSelf, TField>(this ILCursor c, string fieldName, Func<TSelf, TField> func)
        {
            while (c.TryGotoNext(MoveType.AfterLabel, x => x.MatchLdfld<TSelf>(fieldName)))
            {
                c.Remove();
                c.MoveAfterLabels(); // AfterLabel stuff is probably not needed here, but just to be safe...
                c.EmitDelegate(func);
            }
        }

        public static void RemoveMatch(this ILCursor c, params Func<Instruction, bool>[] predicates)
        {
            c.GotoNext(predicates);
            c.RemoveRange(predicates.Length);
        }

        public static void EmitLdloc(this ILCursor c, int index)
        {
            c.Emit(index, OpCodes.Ldloc_0, OpCodes.Ldloc_1, OpCodes.Ldloc_2, OpCodes.Ldloc_3, OpCodes.Ldloc_S, OpCodes.Ldloc);
        }

        public static void EmitStloc(this ILCursor c, int index)
        {
            c.Emit(index, OpCodes.Stloc_0, OpCodes.Stloc_1, OpCodes.Stloc_2, OpCodes.Stloc_3, OpCodes.Stloc_S, OpCodes.Stloc);
        }

        public static void EmitLdloca(this ILCursor c, int index)
        {
            c.Emit(index, OpCodes.Ldloca_S, OpCodes.Ldloca);
        }

        public static void EmitLdarg(this ILCursor c, int index)
        {
            c.Emit(index, OpCodes.Ldarg_0, OpCodes.Ldarg_1, OpCodes.Ldarg_2, OpCodes.Ldarg_3, OpCodes.Ldarg_S, OpCodes.Ldarg);
        }

        public static void EmitStarg(this ILCursor c, int index)
        {
            c.Emit(index, OpCodes.Starg_S, OpCodes.Starg);
        }

        public static void EmitLdarga(this ILCursor c, int index)
        {
            c.Emit(index, OpCodes.Ldarga_S, OpCodes.Ldarga);
        }

        public static void GotoOnly(this ILCursor c, params Func<Instruction, bool>[] predicates)
        {
            c.Index = 0;
            c.GotoNext(predicates);
            c.Index++;
            if (c.TryFindNext(out _, predicates))
            {
                throw new InvalidOperationException("Multiple matches found for given predicates");
            }
            c.Index--;
        }

        private static ILCursor Emit(this ILCursor c, int index, OpCode _0, OpCode _1, OpCode _2, OpCode _3, OpCode _S, OpCode _default)
        {
            return index switch
            {
                0 => c.Emit(_0),
                1 => c.Emit(_1),
                2 => c.Emit(_2),
                3 => c.Emit(_3),
                _ => c.Emit(index, _S, _default),
            };
        }

        private static ILCursor Emit(this ILCursor c, int index, OpCode _S, OpCode _default)
        {
            if (index < 256)
            {
                return c.Emit(_S, (byte)index);
            }
            else
            {
                return c.Emit(_default, index);
            }
        }
    }
}
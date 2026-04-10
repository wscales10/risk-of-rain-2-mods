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
    }
}

using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace AssortedExperiments
{
    public static class ILExtensions
    {
        public static bool MatchGet<T>(this Instruction instr, string propertyName)
        {
            return instr.MatchCall<T>($"get_{propertyName}");
        }

        public static bool MatchGetVirt<T>(this Instruction instr, string propertyName)
        {
            return instr.MatchCallvirt<T>($"get_{propertyName}");
        }

        public static void EmitGet<T>(this ILCursor c, string propertyName)
        {
            c.Emit<T>(OpCodes.Call, $"get_{propertyName}");
        }

        public static void EmitGetVirt<T>(this ILCursor c, string propertyName)
        {
            c.Emit<T>(OpCodes.Callvirt, $"get_{propertyName}");
        }

        public static bool MatchSet<T>(this Instruction instr, string propertyName)
        {
            return instr.MatchCall<T>($"set_{propertyName}");
        }

        public static bool MatchSetVirt<T>(this Instruction instr, string propertyName)
        {
            return instr.MatchCallvirt<T>($"set_{propertyName}");
        }

        public static void EmitSet<T>(this ILCursor c, string propertyName)
        {
            c.Emit<T>(OpCodes.Call, $"set_{propertyName}");
        }

        public static void EmitSetVirt<T>(this ILCursor c, string propertyName)
        {
            c.Emit<T>(OpCodes.Callvirt, $"set_{propertyName}");
        }

        public static void EmitPredicate<T>(this ILCursor c, Func<T, bool> predicate)
        {
            c.EmitDelegate(predicate);
        }
    }
}
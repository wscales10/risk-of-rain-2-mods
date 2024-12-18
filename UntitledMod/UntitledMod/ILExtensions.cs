using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Linq;
using System.Reflection;

namespace UntitledMod
{
    public static class ILExtensions
    {
        public static ILCursor GoToLast(this ILCursor cursor, params Func<Instruction, bool>[] predicates)
        {
            cursor.GotoNext(predicates);

            do
            {
                System.Diagnostics.Debug.WriteLine(cursor.Index);
            } while (cursor.TryGotoNext(predicates));

            return cursor;
        }

        public static EventInfo AddHook(this EventInfo e, object target, string methodName)
        {
            e.AddEventHandler(null, Delegate.CreateDelegate(e.EventHandlerType, target, methodName));
            return e;
        }

        public static VariableDefinition GetVariable<T>(this ILContext il, int index)
        {
            var output = il.Body.Variables.Single(x => x.Index == index);

            if (output.VariableType.Name != typeof(T).Name)
            {
                throw new InvalidOperationException($"'{output.VariableType.Name}' != '{typeof(T).Name}'");
            }

            return output;
        }

        public static string GetModifiedIL(this ILContext il)
        {
            return string.Join("\r\n", il.Body.Instructions.Select(ConvertInstructionToString));
        }

        private static string ConvertInstructionToString(Instruction instr, int index)
        {
            try
            {
                if (instr.Operand is ILLabel label)
                {
                    return Instruction.Create(instr.OpCode, label.Target).ToString();
                }

                if (instr.OpCode == OpCodes.Call && ((MethodReference)instr.Operand).HasThis)
                {
                    var output = instr.ToString();
                    return output.Insert(output.IndexOf("call ") + 5, "instance ");
                }

                return instr.ToString();
            }
            catch (Exception ex)
            {
                return $"[{index}]: {ex.Message}";
            }
        }
    }
}
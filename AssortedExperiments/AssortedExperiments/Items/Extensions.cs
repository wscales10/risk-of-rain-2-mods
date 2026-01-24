using System;

namespace AssortedExperiments.Items
{
    public static class Extensions
    {
        public static TOut? Then<TIn, TOut>(this TIn? input, Func<TIn, TOut> func)
            where TIn : UnityEngine.Object
            where TOut : class
        {
            if (input)
            {
                return func(input!);
            }

            return null;
        }

        // TODO: try to think of a better name?
        public static TOut? Then2<TIn, TOut>(this TIn? input, Func<TIn, TOut> func)
            where TIn : UnityEngine.Object
            where TOut : struct
        {
            if (input)
            {
                return func(input!);
            }

            return null;
        }
    }
}
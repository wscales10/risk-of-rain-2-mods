using System;

namespace UntitledMod.Context
{
    internal static class ContextExtensions
    {
        public static void ThrowIfServer(this IRoR2Context roR2Context)
        {
            if (roR2Context.IsNetworkServerActive)
            {
                throw new InvalidOperationException();
            }
        }

        public static void ThrowIfClient(this IRoR2Context roR2Context)
        {
            if (!roR2Context.IsNetworkServerActive)
            {
                throw new InvalidOperationException();
            }
        }
    }
}
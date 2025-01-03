using UnityEngine.Networking;

namespace UntitledMod.Context
{
    internal class RoR2Context : IRoR2Context
    {
        public bool IsNetworkServerActive => NetworkServer.active;
    }
}

using UnityEngine.Networking;

namespace UntitledMod
{
    internal class RoR2Context : IRoR2Context
    {
        public bool IsNetworkServerActive => NetworkServer.active;
    }
}

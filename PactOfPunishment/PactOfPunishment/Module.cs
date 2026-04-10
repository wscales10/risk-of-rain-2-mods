using BepInEx.Configuration;
using BepInEx.Logging;

namespace PactOfPunishment
{
    public abstract class Module
    {
        public ManualLogSource Logger { get; internal set; }

        public ConfigFile Config { get; internal set; }

        public abstract void Init();
    }
}
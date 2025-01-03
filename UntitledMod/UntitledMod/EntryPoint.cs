using BepInEx;
using R2API.Networking;
using UntitledMod.Tests;

namespace UntitledMod
{
    [BepInPlugin("com.woodyscales.untitledmod", "Untitled Mod", "0.0.1")]
    [BepInDependency("com.bepis.r2api")]
    [BepInDependency(NetworkingAPI.PluginGUID)]
    public class EntryPoint : BaseUnityPlugin
    {
        public UntitledMod Mod { get; private set; }

        public void Awake()
        {
            var logger = new CustomLogger(this.Logger);
            new TestsEntryPoint(logger).RunAllTests();
            this.Mod = DependencyInjection.BuildMod(logger);
        }
    }
}
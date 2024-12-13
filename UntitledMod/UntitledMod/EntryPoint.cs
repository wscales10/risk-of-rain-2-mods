using BepInEx;

namespace UntitledMod
{
    [BepInPlugin("com.woodyscales.untitledmod", "Untitled Mod", "0.0.1")]
    public class EntryPoint : BaseUnityPlugin
    {
        public UntitledMod Mod { get; private set; }

        public void Awake()
        {
            var customLogger = new CustomLogger(this.Logger);
            this.Mod = new UntitledMod(customLogger, () => new InventoryManager(customLogger));
        }
    }
}
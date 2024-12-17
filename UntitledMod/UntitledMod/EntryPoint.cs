using BepInEx;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace UntitledMod
{
    [BepInPlugin("com.woodyscales.untitledmod", "Untitled Mod", "0.0.1")]
    public class EntryPoint : BaseUnityPlugin
    {
        public UntitledMod Mod { get; private set; }

        public void Awake()
        {
            var services = new ServiceCollection();

            services.AddSingleton(new CustomLogger(this.Logger));
            services.AddFactory<IInventoryManager, InventoryManager>();
            services.AddSingleton<ServerSide>();
            services.AddSingleton<Writer>();
            services.AddSingleton<InventoriesInfo>();
            services.AddSingleton<Reader>();

            services.AddSingleton<UntitledMod>();

            this.Mod = services.BuildServiceProvider().GetRequiredService<UntitledMod>();
        }
    }
}
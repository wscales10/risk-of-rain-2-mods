using Microsoft.Extensions.DependencyInjection;
using RoR2;
using System;
using UntitledMod.Context;

namespace UntitledMod
{
    public static class DependencyInjection
    {
        public static UntitledMod BuildMod(ICustomLogger logger)
        {
            var services = new ServiceCollection();

            services.AddSingleton<UntitledMod>()
                .AddSingleton<WriterHooks>()
                    .AddSingleton(logger)
                    .AddSingleton<Writer>()
                        .AddSingleton<IInventoryManagers, InventoryManagers>()
                            .AddFactory<IInventoryManager, InventoryManager>()
                        .AddSingleton<IPickupWeightMultipliers, PickupWeightMultipliers>()
                        .AddSingleton<ServerSide>()
                            .AddSingleton<IRoR2Context, RoR2Context>()
                        .AddSingleton<Func<ItemIndex, PickupIndex>>(PickupCatalog.FindPickupIndex)
                .AddSingleton<ReaderHooks>()
                    .AddSingleton<Reader>()
                        .AddSingleton<InventoriesInfo>();

            return services.BuildServiceProvider().GetRequiredService<UntitledMod>();
        }

        internal static IServiceCollection AddFactory<TService, TImplementation>(this IServiceCollection services)
                    where TService : class
            where TImplementation : class, TService
        {
            return services.AddTransient<TService, TImplementation>().AddTransient<Func<TService>>(sp =>
                 () => sp.GetRequiredService<TService>()
            );
        }
    }
}
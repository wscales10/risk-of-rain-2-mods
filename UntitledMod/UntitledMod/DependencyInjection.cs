using Microsoft.Extensions.DependencyInjection;
using RoR2;
using System;

namespace UntitledMod
{
    public static class DependencyInjection
    {
        public static UntitledMod BuildMod(ICustomLogger logger)
        {
            var services = new ServiceCollection();

            services.AddSingleton<UntitledMod>()
                .AddSingleton<Writer>()
                    .AddSingleton(logger)
                    .AddSingleton<IInventoryManagers, InventoryManagers>()
                        .AddFactory<IInventoryManager, InventoryManager>()
                    .AddSingleton<IPickupWeightMultipliers, PickupWeightMultipliers>()
                    .AddSingleton<ServerSide>()
                    .AddSingleton<Func<ItemIndex, PickupIndex>>(PickupCatalog.FindPickupIndex)
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
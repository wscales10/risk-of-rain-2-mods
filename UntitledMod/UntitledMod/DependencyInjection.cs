using Microsoft.Extensions.DependencyInjection;
using System;

namespace UntitledMod
{
    internal static class DependencyInjection
    {
        public static IServiceCollection AddFactory<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            return services.AddTransient<TService, TImplementation>().AddTransient<Func<TService>>(sp =>
                 () => sp.GetRequiredService<TService>()
            );
        }
    }
}
using Microsoft.Extensions.DependencyInjection;
using Miha.Shared.ZonedClocks;
using Miha.Shared.ZonedClocks.Interfaces;
using NodaTime;

namespace Miha.Shared;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddClocks(this IServiceCollection services)
    {
        services.AddSingleton<IClock>(SystemClock.Instance);
        services.AddSingleton<IEasternStandardZonedClock, EasternStandardZonedClock>();

        return services;
    }
}

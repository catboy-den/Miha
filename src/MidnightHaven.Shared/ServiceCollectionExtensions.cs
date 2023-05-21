using Microsoft.Extensions.DependencyInjection;
using MidnightHaven.Shared.ZonedClocks;
using MidnightHaven.Shared.ZonedClocks.Interfaces;
using NodaTime;

namespace MidnightHaven.Shared;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddClocks(this IServiceCollection services)
    {
        services.AddSingleton<IClock>(SystemClock.Instance);
        services.AddSingleton<IEasternStandardZonedClock, EasternStandardZonedClock>();

        return services;
    }
}

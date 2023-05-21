using Microsoft.Extensions.DependencyInjection;
using MidnightHaven.Background.Services;

namespace MidnightHaven.Background;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        services.AddHostedService<BirthdayScannerService>();

        return services;
    }
}

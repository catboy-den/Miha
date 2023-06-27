using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MidnightHaven.Chan.Health;
using MidnightHaven.Chan.Services;
using MidnightHaven.Discord;
using MidnightHaven.Logic;
using MidnightHaven.Redis;
using MidnightHaven.Shared;
using TinyHealthCheck;

namespace MidnightHaven.Chan;

public static class Startup
{
    public static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services.Configure<HostOptions>(hostOptions =>
        {
            hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
        });

        services.AddClocks();
        services.AddHealthServices();

        services.AddRedis(context.Configuration);

        services
            .AddDiscordOptions(context.Configuration)
            .AddDiscordClientServices()
            .AddDiscordMessageBus();

        services
            .AddLogicServices()
            .AddBackgroundServices();
    }

    private static IServiceCollection AddHealthServices(this IServiceCollection services)
    {
        services.AddCustomTinyHealthCheck<ReadinessHealthCheck>(config =>
        {
            config.Port = 8000;
            config.Hostname = "*";
            config.UrlPath = "/readiness";

            return config;
        });

        return services;
    }

    private static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        services.AddHostedService<BirthdayScannerService>();

        return services;
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Miha.Logic;
using Miha.Redis;
using Miha.Discord;
using Miha.Health;
using Miha.Services;
using Miha.Shared;
using TinyHealthCheck;

namespace Miha;

public static class Startup
{
    public static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services.Configure<HostOptions>(hostOptions =>
        {
            hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
        });

        services.AddClocks();
        services.AddHealthServices(context.Configuration);

        services.AddRedis(context.Configuration);

        services
            .AddDiscordOptions(context.Configuration)
            .AddDiscordServices()
            .AddDiscordHostedServices()
            .AddDiscordMessageBus();

        services
            .AddLogicServices()
            .AddBackgroundServices();
    }

    private static IServiceCollection AddHealthServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<HealthOptions>().Bind(configuration.GetSection(HealthOptions.Section));

        var options = configuration.GetSection(HealthOptions.Section).Get<HealthOptions>();

        options ??= new HealthOptions();

        services.AddCustomTinyHealthCheck<MihaHealthCheck>(config =>
        {
            config.Port = options.Port;
            config.Hostname = "*";
            config.UrlPath = options.Endpoint;
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

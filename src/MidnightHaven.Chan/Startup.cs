using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MidnightHaven.Background;
using MidnightHaven.Discord;
using MidnightHaven.Logic;
using MidnightHaven.Redis;
using MidnightHaven.Shared;

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

        services.AddRedis(context.Configuration);

        services
            .AddDiscordOptions(context.Configuration)
            .AddDiscordClientServices()
            .AddDiscordMessageBus();

        services
            .AddLogicServices()
            .AddBackgroundServices();
    }
}

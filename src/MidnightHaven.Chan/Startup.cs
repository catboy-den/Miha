using System.Reflection;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MidnightHaven.Chan.Consumers;
using MidnightHaven.Chan.Consumers.GuildEvent;
using MidnightHaven.Chan.Services;
using MidnightHaven.Redis;
using SlimMessageBus.Host.Memory;
using SlimMessageBus.Host.MsDependencyInjection;

namespace MidnightHaven.Chan;

public static class Startup
{
    public static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services.AddBackgroundServices();
        services.AddDiscordDotNet(context.Configuration);

        services.AddSlimMessageBus(mb =>
        {
            mb
                .Produce<IGuildScheduledEvent>(x => x.DefaultTopic("guildevent"))
                .Consume<IGuildScheduledEvent>(x => x
                    .Topic(Topics.GuildEvent.Cancelled).WithConsumer<GuildEventCancelledConsumer>())
                .Consume<IGuildScheduledEvent>(x => x
                    .Topic(Topics.GuildEvent.Created).WithConsumer<GuildEventCreatedConsumer>())
                .Consume<IGuildScheduledEvent>(x => x
                    .Topic(Topics.GuildEvent.Started).WithConsumer<GuildEventStartedConsumer>())

                .WithProviderMemory();
        }, addConsumersFromAssembly: new[] { Assembly.GetExecutingAssembly() }); // Auto discover consumers and register inside DI container);

        services.AddRedis(context.Configuration);
    }

    private static IServiceCollection AddDiscordDotNet(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DiscordOptions>().Bind(configuration.GetSection(DiscordOptions.Section));
        services.AddSingleton(new DiscordSocketClient());
        services.AddSingleton<DiscordSocketClient>();

        return services;
    }

    private static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        services.AddHostedService<BotService>();

        return services;
    }
}

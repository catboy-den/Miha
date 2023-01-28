using System.Reflection;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using MidnightHaven.Chan.Consumers.GuildEvents;
using MidnightHaven.Chan.Services;
using SlimMessageBus.Host.Memory;
using SlimMessageBus.Host.MsDependencyInjection;

namespace MidnightHaven.Chan;

public static class Startup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddBackgroundServices();
        services.AddDiscordDotNet();

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
    }

    private static IServiceCollection AddDiscordDotNet(this IServiceCollection services)
    {
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

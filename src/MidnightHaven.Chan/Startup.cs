using System.Reflection;
using Discord;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MidnightHaven.Chan.Consumers;
using MidnightHaven.Chan.Consumers.GuildEvent;
using MidnightHaven.Chan.Models.Options;
using MidnightHaven.Chan.Services.Client;
using MidnightHaven.Chan.Services.Handlers;
using MidnightHaven.Chan.Services.Logic;
using MidnightHaven.Chan.Services.Logic.Interfaces;
using MidnightHaven.Redis;
using SlimMessageBus.Host.Memory;
using SlimMessageBus.Host.MsDependencyInjection;

namespace MidnightHaven.Chan;

public static class Startup
{
    public static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services.AddOptions(context.Configuration);
        services.AddRedis(context.Configuration);

        services.AddLogicServices();
        services.AddSlimMessageBus();

        services.AddDiscordClientServices();
    }

    private static IServiceCollection AddSlimMessageBus(this IServiceCollection services)
    {
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
                .Consume<IGuildScheduledEvent>(x => x
                    .Topic(Topics.GuildEvent.Updated).WithConsumer<GuildEventUpdatedConsumer>())

                .WithProviderMemory();
        }, addConsumersFromAssembly: new[] { Assembly.GetExecutingAssembly() }); // Auto discover consumers and register inside DI container);
        return services;
    }

    private static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DiscordOptions>().Bind(configuration.GetSection(DiscordOptions.Section));

        return services;
    }

    private static IServiceCollection AddDiscordClientServices(this IServiceCollection services)
    {
        services.AddHostedService<InteractionHandler>();

        services.AddHostedService<GuildEventMonitorService>();
        services.AddHostedService<SlimMessageBusService>();

        return services;
    }

    private static IServiceCollection AddLogicServices(this IServiceCollection services)
    {
        services.AddTransient<IGuildService, GuildService>();

        return services;
    }
}

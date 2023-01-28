using System.Reflection;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using MidnightHaven.Chan.Consumers;
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

            /*// Any consumer events for the bus (a sum of all events across registered consumers)
            .AttachEvents(events =>
        {
            // Invoke the action for the specified message type when sent via the bus:
            events.OnMessageArrived = (bus, consumerSettings, message, path, nativeMessage) => {
                Console.WriteLine("The message: {0} arrived on the topic/queue {1}", message, path);
            };

            events.OnMessageFault = (bus, consumerSettings, message, ex, nativeMessage) => {
            };

            events.OnMessageFinished = (bus, consumerSettings, message, path, nativeMessage) => {
                Console.WriteLine("The SomeMessage: {0} finished on the topic/queue {1}", message, path);
            }
        });*/
        services.AddSlimMessageBus(mb =>
        {
            mb
                .Produce<IGuildScheduledEvent>(x => x.DefaultTopic("test"))
                .Consume<IGuildScheduledEvent>(x => x
                    .Topic(Topics.GuildEvent.Created)
                    .WithConsumer<GuildEventCreatedConsumer>())
                .Consume<IGuildScheduledEvent>(x => x
                    .Topic(Topics.GuildEvent.Cancelled)
                    .WithConsumer<GuildEventCancelledConsumer>())

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

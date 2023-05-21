using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using MidnightHaven.Discord.Consumers;
using SlimMessageBus;

namespace MidnightHaven.Discord.Services;

public class SlimMessageBusService : DiscordClientService
{
    private readonly IMessageBus _bus;

    public SlimMessageBusService(
        DiscordSocketClient client,
        IMessageBus bus,
        ILogger<SlimMessageBusService> logger) : base(client, logger)
    {
        _bus = bus;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Publish events to our Slim Message Bus consumers
        Client.GuildScheduledEventCreated += @event => _bus.Publish(@event, Topics.GuildEvent.Created, cancellationToken: stoppingToken);
        Client.GuildScheduledEventStarted += @event => _bus.Publish(@event, Topics.GuildEvent.Started, cancellationToken: stoppingToken);
        Client.GuildScheduledEventCancelled += @event => _bus.Publish(@event, Topics.GuildEvent.Cancelled, cancellationToken: stoppingToken);
        Client.GuildScheduledEventUpdated += (_, @event) => _bus.Publish(@event, Topics.GuildEvent.Updated, cancellationToken: stoppingToken);

        await Client.WaitForReadyAsync(stoppingToken);
        await Client.SetActivityAsync(new Game("on v" + ThisAssembly.AssemblyInformationalVersion));
    }
}

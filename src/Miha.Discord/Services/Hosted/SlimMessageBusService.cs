using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Miha.Discord.Consumers;
using Miha.Shared;
using SlimMessageBus;

namespace Miha.Discord.Services.Hosted;

public class SlimMessageBusService(
    DiscordSocketClient client,
    IPublishBus bus,
    ILogger<SlimMessageBusService> logger) : DiscordClientService(client, logger)
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Publish events to our Slim Message Bus consumers
        Client.GuildScheduledEventCreated += @event => bus.Publish(@event, Topics.GuildEvent.Created, cancellationToken: stoppingToken);
        Client.GuildScheduledEventStarted += @event => bus.Publish(@event, Topics.GuildEvent.Started, cancellationToken: stoppingToken);
        Client.GuildScheduledEventCancelled += @event => bus.Publish(@event, Topics.GuildEvent.Cancelled, cancellationToken: stoppingToken);
        Client.GuildScheduledEventUpdated += (_, @event) => bus.Publish(@event, Topics.GuildEvent.Updated, cancellationToken: stoppingToken);

        await Client.WaitForReadyAsync(stoppingToken);
        await Client.SetActivityAsync(new Game("on " + Versioning.GetVersion()));
    }
}

using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Miha.Discord.Extensions;
using Miha.Logic.Services.Interfaces;
using SlimMessageBus;

namespace Miha.Discord.Consumers.GuildEvent;

public class GuildEventCancelledConsumer : IConsumer<IGuildScheduledEvent>
{
    private readonly DiscordSocketClient _client;
    private readonly IGuildService _guildService;
    private readonly ILogger<GuildEventCancelledConsumer> _logger;

    public GuildEventCancelledConsumer(
        DiscordSocketClient client,
        IGuildService guildService,
        ILogger<GuildEventCancelledConsumer> logger)
    {
        _client = client;
        _guildService = guildService;
        _logger = logger;
    }

    public async Task OnHandle(IGuildScheduledEvent guildEvent)
    {
        var loggingChannel = await _guildService.GetLoggingChannelAsync(guildEvent.Guild.Id);
        if (loggingChannel.IsFailed)
        {
            if (loggingChannel.Reasons.Any(m => m.Message == "Logging channel not set"))
            {
                _logger.LogDebug("Guild logging channel not set {GuildId} {EventId}", guildEvent.Guild.Id, guildEvent.Id);
                return;
            }

            _logger.LogInformation("Failed getting logging channel for guild {GuildId} {EventId}", guildEvent.Guild.Id, guildEvent.Id);
            return;
        }

        var location = guildEvent.Location ?? "Unknown";

        if (location is "Unknown" && guildEvent.ChannelId.HasValue)
        {
            location = "Discord";
        }

        var embed = new EmbedBuilder().AsScheduledEvent(
            eventVerb: "Event cancelled",
            eventName: guildEvent.Name,
            eventLocation: location,
            eventDescription: null,
            color: Color.Red,
            authorAvatarUrl: guildEvent.Creator is null ? _client.CurrentUser.GetAvatarUrl() : guildEvent.Creator.GetAvatarUrl(),
            authorUsername: guildEvent.Creator?.Username);

        await loggingChannel.Value.SendMessageAsync(embed: embed.Build());
    }
}

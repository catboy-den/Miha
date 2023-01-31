using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using MidnightHaven.Chan.Extensions;
using MidnightHaven.Chan.Services.Interfaces;
using SlimMessageBus;

namespace MidnightHaven.Chan.Consumers.GuildEvent;

public class GuildEventCancelledConsumer : IConsumer<IGuildScheduledEvent>
{
    private readonly DiscordSocketClient _client;
    private readonly IGuildOptionsService _guildOptionsService;
    private readonly ILogger<GuildEventCancelledConsumer> _logger;

    public GuildEventCancelledConsumer(
        DiscordSocketClient client,
        IGuildOptionsService guildOptionsService,
        ILogger<GuildEventCancelledConsumer> logger)
    {
        _client = client;
        _guildOptionsService = guildOptionsService;
        _logger = logger;
    }

    public async Task OnHandle(IGuildScheduledEvent guildEvent)
    {
        var loggingChannel = await _guildOptionsService.GetLoggingChannel(guildEvent.Guild.Id);
        if (loggingChannel.IsFailed)
        {
            _logger.LogInformation("Failed getting logging channel for guild {GuildId} {EventId}", guildEvent.Guild.Id, guildEvent.Id);
            return;
        }

        var description = string.IsNullOrEmpty(guildEvent.Description) ? "`No event description`" : guildEvent.Description;
        var location = guildEvent.Location ?? "Unknown";

        if (location is "Unknown" && guildEvent.ChannelId.HasValue)
        {
            location = "Discord";
        }

        var embed = new EmbedBuilder()
            .WithTitle(location + " - " + guildEvent.Name)
            .WithDescription(description)
            .WithColor(Color.Red)
            .WithVersionFooter()
            .WithCurrentTimestamp();

        if (guildEvent.Creator is null)
        {
            var botAvatarUrl = _client.CurrentUser.GetAvatarUrl();
            embed.WithAuthor("Event cancelled", botAvatarUrl);
        }
        else
        {
            var creatorAvatarUrl = guildEvent.Creator?.GetAvatarUrl();
            embed.WithAuthor(guildEvent.Creator?.Username + " - Event cancelled", creatorAvatarUrl).WithThumbnailUrl(creatorAvatarUrl);
        }

        await loggingChannel.Value.SendMessageAsync(embed: embed.Build());
    }
}

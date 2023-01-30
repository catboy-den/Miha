using Discord;
using Microsoft.Extensions.Logging;
using MidnightHaven.Chan.Services.Interfaces;
using SlimMessageBus;

namespace MidnightHaven.Chan.Consumers.GuildEvent;

public class GuildEventCancelledConsumer : IConsumer<IGuildScheduledEvent>
{
    private readonly IGuildOptionsService _guildOptionsService;
    private readonly ILogger<GuildEventCancelledConsumer> _logger;

    public GuildEventCancelledConsumer(
        IGuildOptionsService guildOptionsService,
        ILogger<GuildEventCancelledConsumer> logger)
    {
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

        var creatorAvatarUrl = guildEvent.Creator?.GetAvatarUrl();
        var description = string.IsNullOrEmpty(guildEvent.Description) ? "`No event description`" : guildEvent.Description;
        var location = guildEvent.Location ?? "Unknown";

        if (location is "Unknown" && guildEvent.ChannelId.HasValue)
        {
            location = "Discord";
        }

        var embed = new EmbedBuilder()
            .WithAuthor(guildEvent.Creator?.Username + " - Event cancelled", creatorAvatarUrl)
            .WithThumbnailUrl(creatorAvatarUrl)
            .WithTitle(location + " - " + guildEvent.Name)
            .WithDescription(description)
            .WithColor(Color.Red)
            .WithFooter("v1.06")
            .WithCurrentTimestamp();

        await loggingChannel.Value.SendMessageAsync(embed: embed.Build());
    }
}

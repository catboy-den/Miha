using Discord;
using Microsoft.Extensions.Logging;
using MidnightHaven.Chan.Helpers;
using MidnightHaven.Chan.Services.Interfaces;
using SlimMessageBus;

namespace MidnightHaven.Chan.Consumers.GuildEvent;

public class GuildEventCreatedConsumer : IConsumer<IGuildScheduledEvent>
{
    private readonly IGuildOptionsService _guildOptionsService;
    private readonly ILogger<GuildEventCreatedConsumer> _logger;

    public GuildEventCreatedConsumer(
        IGuildOptionsService guildOptionsService,
        ILogger<GuildEventCreatedConsumer> logger)
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

        // Sometimes the creator can be null in the event, this will go grab the event AGAIN
        if (guildEvent.Creator is null)
        {
            guildEvent = await guildEvent.Guild.GetEventAsync(guildEvent.Id);
            if (guildEvent.Creator is null)
            {
                _logger.LogError("Guild scheduled event is null {GuildId} {EventId}", guildEvent.Guild.Id, guildEvent.Id);
                return;
            }
        }

        var creatorAvatarUrl = guildEvent.Creator?.GetAvatarUrl();
        var coverImageUrl = guildEvent.CoverImageId != null ? guildEvent.GetCoverImageUrl().Replace($"/{guildEvent.Guild.Id}", "") : null;
        var description = string.IsNullOrEmpty(guildEvent.Description) ? "`No event description`" : guildEvent.Description;
        var location = guildEvent.Location ?? "Unknown";
        string? voiceChannel = null;

        if (location is "Unknown" && guildEvent.ChannelId.HasValue)
        {
            var channel = await guildEvent.Guild.GetVoiceChannelAsync(guildEvent.ChannelId.Value);
            voiceChannel = channel?.Name;
            location = "Discord";
        }

        var fields = new List<EmbedFieldBuilder>
        {
            new EmbedFieldBuilder()
                .WithName("Start time")
                .WithValue(
                    guildEvent.StartTime.ToDiscordTimestamp(DiscordTimestampHelper.Style.LongDateTime) + " - " +
                    guildEvent.StartTime.ToDiscordTimestamp(DiscordTimestampHelper.Style.RelativeTime))
                .WithIsInline(false)
        };

        if (guildEvent.EndTime is not null)
        {
            fields.Add(new EmbedFieldBuilder()
                .WithName("End time")
                .WithValue(
                    guildEvent.EndTime.ToDiscordTimestamp(DiscordTimestampHelper.Style.LongDateTime) + " - " +
                    guildEvent.EndTime.ToDiscordTimestamp(DiscordTimestampHelper.Style.RelativeTime))
                .WithIsInline(false));
        }

        if (voiceChannel is not null)
        {
            fields.Add(new EmbedFieldBuilder()
                .WithName("Voice channel")
                .WithValue(voiceChannel)
                .WithIsInline(false));
        }

        var embed = new EmbedBuilder()
            .WithAuthor(guildEvent.Creator?.Username + " - Event created", creatorAvatarUrl)
            .WithThumbnailUrl(creatorAvatarUrl)
            .WithTitle(location + " - " + guildEvent.Name)
            .WithDescription(description)
            .WithColor(Color.Purple)
            .WithFields(fields)
            .WithFooter("v1.06")
            .WithCurrentTimestamp();

        if (coverImageUrl is not null)
        {
            embed.WithImageUrl(coverImageUrl);
        }

        await loggingChannel.Value.SendMessageAsync(embed: embed.Build());
    }
}

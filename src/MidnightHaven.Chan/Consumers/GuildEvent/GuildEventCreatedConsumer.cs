using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using MidnightHaven.Chan.Extensions;
using MidnightHaven.Chan.Helpers;
using MidnightHaven.Chan.Services.Interfaces;
using SlimMessageBus;

namespace MidnightHaven.Chan.Consumers.GuildEvent;

public class GuildEventCreatedConsumer : IConsumer<IGuildScheduledEvent>
{
    private readonly DiscordSocketClient _client;
    private readonly IGuildOptionsService _guildOptionsService;
    private readonly ILogger<GuildEventCreatedConsumer> _logger;

    public GuildEventCreatedConsumer(
        DiscordSocketClient client,
        IGuildOptionsService guildOptionsService,
        ILogger<GuildEventCreatedConsumer> logger)
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

        var creatorAvatarUrl = guildEvent.Creator?.GetAvatarUrl();
        var coverImageUrl = guildEvent.CoverImageId != null ? guildEvent.GetCoverImageUrl().Replace($"/{guildEvent.Guild.Id}", "") : null;
        var description = string.IsNullOrEmpty(guildEvent.Description) ? "`No event description`" : guildEvent.Description;
        var location = guildEvent.Location ?? "Unknown";
        string? voiceChannel = null;

        if (location is "Unknown" && guildEvent.ChannelId.HasValue)
        {
            voiceChannel = (guildEvent as SocketGuildEvent)?.Channel.Name;
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
            .WithTitle(location + " - " + guildEvent.Name)
            .WithDescription(description)
            .WithColor(Color.Purple)
            .WithFields(fields)
            .WithVersionFooter()
            .WithCurrentTimestamp();

        if (guildEvent.Creator is not null)
        {
            var botAvatarUrl = _client.CurrentUser.GetAvatarUrl();
            embed.WithAuthor("Event created", botAvatarUrl);
        }
        else
        {
            embed
                .WithAuthor(guildEvent.Creator?.Username + " - Event created", creatorAvatarUrl)
                .WithThumbnailUrl(creatorAvatarUrl);
        }

        if (coverImageUrl is not null)
        {
            embed.WithImageUrl(coverImageUrl);
        }

        await loggingChannel.Value.SendMessageAsync(embed: embed.Build());
    }
}

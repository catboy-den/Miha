using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using MidnightHaven.Discord.Extensions;
using MidnightHaven.Logic.Services.Interfaces;
using SlimMessageBus;

namespace MidnightHaven.Discord.Consumers.GuildEvent;

public class GuildEventStartedConsumer : IConsumer<IGuildScheduledEvent>
{
    private readonly DiscordSocketClient _client;
    private readonly IGuildService _guildService;
    private readonly ILogger<GuildEventStartedConsumer> _logger;

    public GuildEventStartedConsumer(
        DiscordSocketClient client,
        IGuildService guildService,
        ILogger<GuildEventStartedConsumer> logger)
    {
        _client = client;
        _guildService = guildService;
        _logger = logger;
    }

    public async Task OnHandle(IGuildScheduledEvent guildEvent)
    {
        var announcementRole = await _guildService.GetAnnouncementRoleAsync(guildEvent.Guild.Id);
        var announcementChannel = await _guildService.GetAnnouncementChannelAsync(guildEvent.Guild.Id);
        if (announcementChannel.IsFailed)
        {
            _logger.LogInformation("Failed getting announcement channel for guild {GuildId} {EventId}", guildEvent.Guild.Id, guildEvent.Id);
            return;
        }

        var coverImageUrl = guildEvent.CoverImageId != null ? guildEvent.GetCoverImageUrl().Replace($"/{guildEvent.Guild.Id}", "") : null;
        var location = guildEvent.Location ?? "Unknown";
        string? voiceChannel = null;

        if (location is "Unknown" && guildEvent.ChannelId.HasValue)
        {
            voiceChannel = (guildEvent as SocketGuildEvent)?.Channel.Name;
            location = "Discord";
        }

        var fields = new List<EmbedFieldBuilder>();

        if (guildEvent.EndTime is not null)
        {
            fields.Add(new EmbedFieldBuilder()
                .WithName("Ends")
                .WithValue(
                    guildEvent.EndTime.ToDiscordTimestamp(TimestampTagStyles.LongDateTime) + " - " +
                    guildEvent.EndTime.ToDiscordTimestamp(TimestampTagStyles.Relative))
                .WithIsInline(false));
        }

        if (voiceChannel is not null)
        {
            fields.Add(new EmbedFieldBuilder()
                .WithName("Voice channel")
                .WithValue(voiceChannel)
                .WithIsInline(false));
        }

        var embed = new EmbedBuilder().AsScheduledEvent(
            eventVerb: "Event starting!",
            eventName: guildEvent.Name,
            eventLocation: location,
            eventDescription: guildEvent.Description,
            eventImageUrl: coverImageUrl,
            color: Color.Green,
            authorAvatarUrl: guildEvent.Creator is null ? _client.CurrentUser.GetAvatarUrl() : guildEvent.Creator.GetAvatarUrl(),
            authorUsername: guildEvent.Creator?.Username,
            fields: fields);

        await announcementChannel.Value.SendMessageAsync(announcementRole.ValueOrDefault?.Mention, embed: embed.Build());
    }
}

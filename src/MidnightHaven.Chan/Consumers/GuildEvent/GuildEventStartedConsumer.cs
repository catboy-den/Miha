using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using MidnightHaven.Chan.Helpers;
using MidnightHaven.Chan.Services.Interfaces;
using SlimMessageBus;

namespace MidnightHaven.Chan.Consumers.GuildEvent;

public class GuildEventStartedConsumer : IConsumer<IGuildScheduledEvent>
{
    private readonly DiscordSocketClient _client;
    private readonly IGuildOptionsService _guildOptionsService;
    private readonly ILogger<GuildEventStartedConsumer> _logger;

    public GuildEventStartedConsumer(
        DiscordSocketClient client,
        IGuildOptionsService guildOptionsService,
        ILogger<GuildEventStartedConsumer> logger)
    {
        _client = client;
        _guildOptionsService = guildOptionsService;
        _logger = logger;
    }

    public async Task OnHandle(IGuildScheduledEvent guildEvent)
    {
        var announcementChannel = await _guildOptionsService.GetAnnouncementChannelAsync(guildEvent.Guild.Id);
        if (announcementChannel.IsFailed)
        {
            _logger.LogInformation("Failed getting announcement channel for guild {GuildId} {EventId}", guildEvent.Guild.Id, guildEvent.Id);
            return;
        }

        var announcementRole = await _guildOptionsService.GetAnnouncementRoleAsync(guildEvent.Guild.Id);
        if (announcementRole.IsFailed)
        {
            _logger.LogWarning("Failed getting announcement role for guild {GuildId} {EventId}", guildEvent.Guild.Id, guildEvent.Id);
        }

        var coverImageUrl = guildEvent.CoverImageId != null ? guildEvent.GetCoverImageUrl().Replace($"/{guildEvent.Guild.Id}", "") : null;
        var description = string.IsNullOrEmpty(guildEvent.Description) ? "`No event description`" : guildEvent.Description;
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

        var embed = EmbedHelper.ScheduledEvent(
            eventVerb: "Event created",
            eventName: guildEvent.Name,
            eventLocation: location,
            eventDescription: description,
            eventImageUrl: coverImageUrl,
            embedColor: Color.Purple,
            authorAvatarUrl: guildEvent.Creator is null ? _client.CurrentUser.GetAvatarUrl() : guildEvent.Creator.GetAvatarUrl(),
            authorUsername: guildEvent.Creator?.Username,
            fields: fields);

        await announcementChannel.Value.SendMessageAsync(embed: embed.Build());

        if (announcementRole.Value is not null && announcementRole.Value.IsMentionable)
        {
            await announcementChannel.Value.SendMessageAsync(announcementRole.Value.Mention, embed: embed.Build());
        }
        else
        {
            await announcementChannel.Value.SendMessageAsync(embed: embed.Build());
        }
    }
}

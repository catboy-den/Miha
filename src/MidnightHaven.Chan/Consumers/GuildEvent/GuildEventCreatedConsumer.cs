using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using MidnightHaven.Chan.Helpers;
using MidnightHaven.Redis.Services.Interfaces;
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

    public async Task OnHandle(IGuildScheduledEvent message)
    {
        var optionsResult = await _guildOptionsService.GetAsync(message.Guild.Id);

        if (optionsResult.IsFailed)
        {
            _logger.LogWarning("Guild doesn't have any options on event creation {GuildId}", message.Guild.Id);
            return;
        }

        var logChannel = optionsResult.Value?.LogChannel;
        if (logChannel is null)
        {
            _logger.LogInformation("Guild doesn't have a logging channel set {GuildId}", message.Guild.Id);
            return;
        }

        var loggingChannel = (await _client.GetChannelAsync(logChannel.Value)) as ITextChannel;
        if (loggingChannel is null)
        {
            _logger.LogWarning("Guild's logging channel wasn't found, or might not be a Text Channel {GuildId} {LoggingChannelId}", message.Guild.Id, logChannel);
            return;
        }

        // Sometimes the creator can be null in the event, this will go grab the event AGAIN
        if (message.Creator is null)
        {
            message = await message.Guild.GetEventAsync(message.Id);
            if (message.Creator is null)
            {
                _logger.LogWarning("Guild scheduled event is null {GuildId} {EventId}", message.Guild.Id, message.Id);
                return;
            }
        }

        var creatorAvatarUrl = message.Creator.GetAvatarUrl();
        var coverImageUrl = message.GetCoverImageUrl()?.Replace($"/{message.Guild.Id}", "");

        var embed = new EmbedBuilder()
            .WithAuthor(message.Creator.Username, creatorAvatarUrl)
            .WithTitle("Event created")
            .WithDescription("test")
            .WithColor(Color.Purple)
            .WithImageUrl(coverImageUrl ?? creatorAvatarUrl)
            .WithFooter("v1.06")
            .Build();

        await loggingChannel.SendMessageAsync(embed: embed);
    }
}

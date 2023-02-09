﻿using Cronos;
using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.WebSocket;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MidnightHaven.Chan.Helpers;
using MidnightHaven.Chan.Models.Options;
using MidnightHaven.Chan.Services.Logic.Interfaces;

namespace MidnightHaven.Chan.Services.Client;

public partial class GuildEventMonitorService : DiscordClientService
{
    private readonly DiscordOptions _discordOptions;
    private readonly IGuildService _guildService;
    private readonly ILogger<GuildEventMonitorService> _logger;

    private const string Schedule = "0,5,10,15,20,25,30,35,40,45,50,55 ? * * *"; // https://crontab.cronhub.io/

    private readonly IMemoryCache _memoryCache;
    private readonly MemoryCacheEntryOptions _memoryCacheEntryOptions;
    private readonly CronExpression _cron;

    public GuildEventMonitorService(
        DiscordSocketClient client,
        IGuildService guildService,
        IOptions<DiscordOptions> discordOptions,
        ILogger<GuildEventMonitorService> logger) : base(client, logger)
    {
        _discordOptions = discordOptions.Value;
        _guildService = guildService;
        _logger = logger;

        _memoryCache = new MemoryCache(new MemoryCacheOptions { SizeLimit = 32 });
        _memoryCacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSize(1)
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(25))
            .SetSlidingExpiration(TimeSpan.FromMinutes(15));

        _cron = CronExpression.Parse(Schedule, CronFormat.Standard);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Client.WaitForReadyAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckScheduledEventsAsync();

            var utcNow = DateTime.UtcNow;
            var nextUtc = _cron.GetNextOccurrence(utcNow);

            if (nextUtc is null)
            {
                _logger.LogWarning("Next utc occurence is null");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                continue;
            }

            await Task.Delay(nextUtc.Value - utcNow, stoppingToken);
        }
    }

    private async Task CheckScheduledEventsAsync()
    {
        var memCacheStats = _memoryCache.GetCurrentStatistics();

        LogDebugMemCacheStats(memCacheStats?.CurrentEntryCount, memCacheStats?.CurrentEstimatedSize);

        try
        {
            var guild = Client.GetGuild(_discordOptions.Guild!.Value);
            if (guild is null)
            {
                _logger.LogCritical("Guild is null {GuildId}", _discordOptions.Guild.Value);
                return;
            }

            foreach (var guildEvent in guild.Events)
            {
                var startsIn = guildEvent.StartTime - DateTime.UtcNow;

                // "Round" our minutes up
                if (startsIn.Seconds <= 60)
                {
                    startsIn = TimeSpan.FromMinutes(startsIn.Minutes + 1);
                }

                if (startsIn.Minutes is < 5 or > 20 || _memoryCache.TryGetValue(guildEvent.Id, out bool notified) && notified)
                {
                    continue;
                }

                if (guildEvent.Status is GuildScheduledEventStatus.Active)
                {
                    _memoryCache.Set(guildEvent.Id, true, _memoryCacheEntryOptions);
                    continue;
                }

                var announcementChannel = await _guildService.GetAnnouncementChannelAsync(guild.Id);
                if (announcementChannel.IsFailed)
                {
                    _logger.LogInformation("Failed getting announcement channel for guild {GuildId} {EventId}", guild.Id, guildEvent.Id);
                    continue;
                }

                var location = guildEvent.Location ?? "Unknown";
                string? voiceChannel = null;

                if (location is "Unknown" && guildEvent.Channel is not null)
                {
                    voiceChannel = guildEvent.Channel.Name;
                    location = "Discord";
                }

                var fields = new List<EmbedFieldBuilder>();
                if (voiceChannel is not null)
                {
                    fields.Add(new EmbedFieldBuilder()
                        .WithName("Voice channel")
                        .WithValue(voiceChannel)
                        .WithIsInline(false));
                }
                else
                {
                    fields.Add(new EmbedFieldBuilder()
                        .WithName("Hosted by")
                        .WithValue(guildEvent.Creator?.Username)
                        .WithIsInline(false));
                }

                var embed = EmbedHelper.ScheduledEvent(
                    eventVerb: "Event is starting soon!",
                    eventName: guildEvent.Name + " - " + guildEvent.StartTime.ToDiscordTimestamp(TimestampTagStyles.Relative),
                    eventLocation: location,
                    eventDescription: string.Empty,
                    embedColor: Color.DarkBlue,
                    authorAvatarUrl: guildEvent.Creator is null ? Client.CurrentUser.GetAvatarUrl() : guildEvent.Creator.GetAvatarUrl(),
                    authorUsername: guildEvent.Creator?.Username,
                    fields: fields);

                await announcementChannel.Value.SendMessageAsync(embed: embed.Build());

                _memoryCache.Set(guildEvent.Id, true, _memoryCacheEntryOptions);
            }
        }
        catch (Exception e)
        {
            LogError(e);
        }
    }

    [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Guild event monitor service memCache stats {entryCount} {estimatedSize}")]
    public partial void LogDebugMemCacheStats(long? entryCount, long? estimatedSize);

    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Exception occurred in GuildEventMonitorService")]
    public partial void LogError(Exception e);
}
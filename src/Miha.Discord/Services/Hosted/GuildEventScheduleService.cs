﻿using System.Globalization;
using System.Text;
using Cronos;
using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.WebSocket;
using FluentAssertions.Common;
using Humanizer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Miha.Discord.Extensions;
using Miha.Discord.Services.Interfaces;
using Miha.Logic.Services.Interfaces;
using Miha.Shared.ZonedClocks.Interfaces;
using NodaTime;
using NodaTime.Extensions;

namespace Miha.Discord.Services.Hosted;

public partial class GuildEventScheduleService : DiscordClientService
{
    private readonly DiscordSocketClient _client;
    private readonly IEasternStandardZonedClock _easternStandardZonedClock;
    private readonly IGuildService _guildService;
    private readonly IGuildScheduledEventService _scheduledEventService;
    private readonly DiscordOptions _discordOptions;
    private readonly ILogger<GuildEventScheduleService> _logger;
    private const string Schedule = "0,5,10,15,20,25,30,35,40,45,50,55 * * * *"; // https://crontab.cronhub.io/

    private readonly CronExpression _cron;

    public GuildEventScheduleService(
        DiscordSocketClient client,
        IEasternStandardZonedClock easternStandardZonedClock,
        IGuildService guildService,
        IGuildScheduledEventService scheduledEventService,
        IOptions<DiscordOptions> discordOptions,
        ILogger<GuildEventScheduleService> logger) : base(client, logger)
    {
        _client = client;
        _easternStandardZonedClock = easternStandardZonedClock;
        _guildService = guildService;
        _scheduledEventService = scheduledEventService;
        _discordOptions = discordOptions.Value;
        _logger = logger;

        _cron = CronExpression.Parse(Schedule, CronFormat.Standard);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Waiting for client to be ready...");
        
        await Client.WaitForReadyAsync(stoppingToken);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            await PostWeeklyScheduleAsync();
            
            var utcNow = _easternStandardZonedClock.GetCurrentInstant().ToDateTimeUtc();
            var nextUtc = _cron.GetNextOccurrence(DateTimeOffset.UtcNow, _easternStandardZonedClock.GetTimeZoneInfo());

            if (nextUtc is null)
            {
                _logger.LogWarning("Next utc occurence is null");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                continue;
            }
            
            var next = nextUtc.Value - utcNow;
            
            _logger.LogDebug("Waiting {Time} until next operation", next.Humanize(3));

            await Task.Delay(nextUtc.Value - utcNow, stoppingToken);
        }
        
        _logger.LogInformation("Hosted service ended");
    }

    private async Task PostWeeklyScheduleAsync()
    {
        if (_discordOptions.Guild is null)
        {
            _logger.LogWarning("Guild isn't configured");
            return;
        }

        var guildResult = await _guildService.GetAsync(_discordOptions.Guild);
        var guild = guildResult.Value;

        if (guildResult.IsFailed || guild is null)
        {
            _logger.LogWarning("Guild doc failed, or the guild is null for some reason {Errors}", guildResult.Errors);
            return;
        }

        if (guild.WeeklyScheduleChannel is null)
        {
            _logger.LogDebug("Guild doesn't have a configured weekly schedule channel");
            return;
        }

        var eventsThisWeekResult = await _scheduledEventService.GetScheduledWeeklyEventsAsync(guild.Id, _easternStandardZonedClock.GetCurrentDate());
        var eventsThisWeek = eventsThisWeekResult.Value;
        
        if (eventsThisWeekResult.IsFailed || eventsThisWeek is null)
        {
            _logger.LogWarning("Fetching this weeks events failed, or is null {Errors}", eventsThisWeekResult.Errors);
            return;
        }
        
        var weeklyScheduleChannelResult = await _guildService.GetWeeklyScheduleChannel(guild.Id);
        var weeklyScheduleChannel = weeklyScheduleChannelResult.Value;
        
        if (weeklyScheduleChannelResult.IsFailed || weeklyScheduleChannel is null)
        {
            _logger.LogWarning("Fetching the guilds weekly schedule channel failed, or is null {Errors}", weeklyScheduleChannelResult.Errors);
            return;
        }

        var daysThisWeek = _easternStandardZonedClock.GetCurrentWeekAsDates();
        
        var eventsByDay = new Dictionary<DateOnly, IList<IGuildScheduledEvent>>();
        var eventsThisWeekList = eventsThisWeek.ToList();
        foreach (var dayOfWeek in daysThisWeek.OrderBy(d => d))
        {
            eventsByDay.Add(dayOfWeek, new List<IGuildScheduledEvent>());
            
            foreach (var guildScheduledEvent in eventsThisWeekList.Where(e => _easternStandardZonedClock.ToZonedDateTime(e.StartTime).Date.ToDateOnly() == dayOfWeek))
            {
                eventsByDay[dayOfWeek].Add(guildScheduledEvent);
            }
        }
        
        _logger.LogInformation("Updating weekly schedule");
        
        var postedHeader = false;
        var postedFooter = false;
        
        var messages = (await weeklyScheduleChannel
            .GetMessagesAsync(50)
            .FlattenAsync())
            .ToList();

        // Wipe any existing messages if a message by day doesn't already exist
        foreach (var (day, _) in eventsByDay)
        {
            var lastPostedMessage = messages
                .FirstOrDefault(m =>
                    m.Author.Id == _client.CurrentUser.Id &&
                    m.Embeds.Any(e => e.Description.Contains(day.ToString("dddd"))));

            if (lastPostedMessage is not null)
            {
                continue;
            }
            
            var messagesToDelete = messages
                .Where(m => m.Author.Id == _client.CurrentUser.Id)
                .ToList();
            
            if (messagesToDelete.Any())
            {
                var deletedMessages = 0;
                
                _logger.LogInformation("Wiping posted messages");

                foreach (var message in messagesToDelete)
                {
                    await message.DeleteAsync();
                    deletedMessages++;
                }

                _logger.LogInformation("Deleted {DeletedMessages} messages", deletedMessages);
            }
            
            break;
        }
        
        // Update (or post) a message with an embed of events for that day, for each day of the week
        foreach (var (day, events) in eventsByDay)
        {
            var embed = new EmbedBuilder();
            var description = new StringBuilder();

            if (!postedHeader && day == eventsByDay.First().Key)
            {
                embed
                    .WithThumbnailUrl(_client.CurrentUser.GetAvatarUrl())
                    .WithAuthor("Weekly event schedule", _client.CurrentUser.GetAvatarUrl());
                postedHeader = true;
            }

            var timeStamp = day
                .ToLocalDate()
                .AtStartOfDayInZone(_easternStandardZonedClock.GetTzdbTimeZone())
                .ToInstant()
                .ToDiscordTimestamp(TimestampTagStyles.ShortDate);
            
            description.AppendLine($"### {day.ToString("dddd")} - {timeStamp}");

            if (!events.Any())
            {
                description.AppendLine("*No events scheduled*");
            }
            else
            {
                foreach (var guildEvent in events.OrderBy(e => e.StartTime))
                {
                    var location = guildEvent.Location ?? "Unknown";
                    var url = $"https://discord.com/events/{guildEvent.Guild.Id}/{guildEvent.Id}";

                    if (location is "Unknown" && guildEvent.ChannelId is not null)
                    {
                        location = "Discord";
                    }

                    description.AppendLine($"- [{location} - {guildEvent.Name}]({url})");

                    description.Append($"  - {guildEvent.StartTime.ToDiscordTimestamp(TimestampTagStyles.ShortTime)}");
                    if (guildEvent.Status is GuildScheduledEventStatus.Active)
                    {
                        description.AppendLine(" - Happening now!");
                    }
                    else
                    {
                        description.AppendLine($" - {guildEvent.StartTime.ToDiscordTimestamp(TimestampTagStyles.Relative)}");
                    }

                    if (guildEvent.Creator is not null)
                    {
                        description.AppendLine($"  - Hosted by {guildEvent.Creator.Mention}");
                    }
                }
            }
            
            if (!postedFooter && day == eventsByDay.Last().Key)
            {
                embed
                    .WithVersionFooter()
                    .WithCurrentTimestamp();

                postedFooter = true;
            }
            
            embed
                .WithColor(new Color(255, 43, 241))
                .WithDescription(description.ToString());

            var lastPostedMessage = messages
                .FirstOrDefault(m =>
                    m.Author.Id == _client.CurrentUser.Id &&
                    m.Embeds.Any(e => e.Description.Contains(day.ToString("dddd"))));
            
            if (lastPostedMessage is null)
            {
                _logger.LogInformation("Posting new message");
                await weeklyScheduleChannel.SendMessageAsync(embed: embed.Build());
            }
            else
            {
                await weeklyScheduleChannel.ModifyMessageAsync(lastPostedMessage.Id, props =>
                {
                    props.Embed = embed.Build();
                });
            }
            
            _logger.LogInformation("Finished updating weekly schedule");
        }
    }
    
    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Exception occurred")]
    public partial void LogError(Exception e);
}

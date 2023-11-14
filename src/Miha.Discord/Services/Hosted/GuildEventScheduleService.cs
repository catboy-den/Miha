using System.Text;
using Cronos;
using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Miha.Discord.Extensions;
using Miha.Discord.Services.Interfaces;
using Miha.Logic.Services.Interfaces;
using Miha.Shared.ZonedClocks.Interfaces;
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
    private const string Schedule = "0,5,10,15,20,25,30,35,40,45,50,55 8-19 * * *"; // https://crontab.cronhub.io/

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
        
        _logger.LogInformation("Trying to post weekly schedule");

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

        var eventsByDay = new Dictionary<string, IList<IGuildScheduledEvent>>();
        foreach (var guildScheduledEvent in eventsThisWeek.OrderBy(e => e.StartTime.Date))
        {
            var day = guildScheduledEvent
                .StartTime
                .ToZonedDateTime()
                .WithZone(_easternStandardZonedClock.GetTzdbTimeZone())
                .Date.AtMidnight().ToDateTimeUnspecified().ToString("dddd");
            
            if (!eventsByDay.ContainsKey(day))
            {
                eventsByDay.Add(day, new List<IGuildScheduledEvent>());
            }

            eventsByDay[day].Add(guildScheduledEvent);
        }

        var postedHeader = false;
        var postedFooter = false;
        
        foreach (var (day, events) in eventsByDay)
        {
            var embed = new EmbedBuilder();
            var description = new StringBuilder();

            if (!postedHeader && day == eventsByDay.First().Key)
            {
                embed.WithAuthor(string.Empty, _client.CurrentUser.GetAvatarUrl());
                description.AppendLine("# Weekly event schedule");
                postedHeader = true;
            }
            
            description.AppendLine("### " + day + " - "  + DiscordTimestampExtensions.ToDiscordTimestamp(events.First().StartTime.Date, TimestampTagStyles.ShortDate));
            
            foreach (var guildEvent in events)
            {
                var location = guildEvent.Location ?? "Unknown";
                var url = $"https://discord.com/events/{guildEvent.Guild.Id}/{guildEvent.Id}";

                if (location is "Unknown" && guildEvent.ChannelId is not null)
                {
                    location = "Discord";
                }

                description.AppendLine($"- [{location} - {guildEvent.Name}]({url})");
                description.AppendLine($"  - {guildEvent.StartTime.ToDiscordTimestamp(TimestampTagStyles.ShortTime)} - {guildEvent.StartTime.ToDiscordTimestamp(TimestampTagStyles.Relative)}");

                if (guildEvent.Creator is not null)
                {
                    description.AppendLine($"  - Hosted by {guildEvent.Creator.Mention}");
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
            
            await weeklyScheduleChannel.SendMessageAsync(embed: embed.Build());

            /*### Monday - <t:1699132740:d>
            - [Discord - Lord of the Rings: Fellowship of the Ring - 20th Anniversary viewing!](https://discord.gg/VqqEgBTe?event=1170558838718603284)
                - <t:1699132740:T> - <t:1699149720:R>
                - Hosted by @Hunt*/
        }
        
        // loop through eventsByDay, order the events by day by start time
    }
    
    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Exception occurred")]
    public partial void LogError(Exception e);
}
